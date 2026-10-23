using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sckc.api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace sckc.api.Extensions
{
    // Reads and writes the trip report files in the data folder. Published reports live in tripreports.data (either a
    // bare array or an object wrapping them in "items"); submissions wait in pendingreports.data until reviewed.
    public static class TripReportStore
    {
        public const string PublishedFile = "data/tripreports.data";
        public const string PendingFile = "data/pendingreports.data";

        // One writer at a time, so two reviews or submissions can't overwrite each other's changes
        private static readonly object WriteLock = new();

        public static List<Report> ReadPublished()
        {
            var filePath = Helper.MapPath(PublishedFile);
            if (!File.Exists(filePath))
            {
                return new List<Report>();
            }

            var items = ReadItems(File.ReadAllText(filePath), PublishedFile, out _);
            var reports = new List<Report>();
            for (var i = 0; i < items.Count; i++)
            {
                try
                {
                    reports.Add(items[i].ToObject<Report>() ?? throw new InvalidDataException("report is null"));
                }
                catch (Exception ex) when (ex is JsonException or InvalidDataException or FormatException)
                {
                    var title = (items[i] as JObject)?["title"]?.ToString();
                    throw new InvalidDataException($"{PublishedFile}: report {i} (\"{title}\") could not be read: {ex.Message}", ex);
                }
            }

            return reports.Where(r => r.published).ToList();
        }

        public static PendingReport AddPending(Report report, string submitterEmail)
        {
            var pending = new PendingReport
            {
                id = report.id,
                token = NewToken(),
                submitted = DateTime.UtcNow,
                submitterEmail = submitterEmail,
                report = report,
            };

            lock (WriteLock)
            {
                var all = ReadPending();
                all.Add(pending);
                WritePending(all);
            }

            return pending;
        }

        // The pending report with this id, but only when the token matches; null for an unknown, reviewed or forged link
        public static PendingReport FindPending(string id, string token)
        {
            return ReadPending().FirstOrDefault(p => p.id == id && TokenMatches(p.token, token));
        }

        // Moves the report into tripreports.data, published. Returns null when the link is not (or no longer) valid.
        public static Report Accept(string id, string token)
        {
            lock (WriteLock)
            {
                var all = ReadPending();
                var pending = all.FirstOrDefault(p => p.id == id && TokenMatches(p.token, token));
                if (pending == null)
                {
                    return null;
                }

                var report = pending.report;
                report.published = true;
                AppendPublished(report);

                all.Remove(pending);
                WritePending(all);
                return report;
            }
        }

        // Discards the submission. Returns null when the link is not (or no longer) valid.
        public static Report Reject(string id, string token)
        {
            lock (WriteLock)
            {
                var all = ReadPending();
                var pending = all.FirstOrDefault(p => p.id == id && TokenMatches(p.token, token));
                if (pending == null)
                {
                    return null;
                }

                all.Remove(pending);
                WritePending(all);
                return pending.report;
            }
        }

        // Used when the notification email can't be sent, so a report nobody knows about doesn't sit there forever
        public static void RemovePending(string id)
        {
            lock (WriteLock)
            {
                var all = ReadPending();
                all.RemoveAll(p => p.id == id);
                WritePending(all);
            }
        }

        private static void AppendPublished(Report report)
        {
            var filePath = Helper.MapPath(PublishedFile);
            var json = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
            var items = ReadItems(json, PublishedFile, out var root);

            // Written field by field so existing entries keep their exact values, and the date keeps the file's yyyy/MM/dd style
            items.Add(new JObject
            {
                ["date"] = report.date.ToString("yyyy/MM/dd"),
                ["title"] = report.title,
                ["report"] = report.report,
                ["trip"] = report.trip,
                ["triptype"] = report.triptype,
                ["organiser"] = report.organiser,
                ["triplength"] = report.triplength,
                ["start"] = report.start,
                ["phrase"] = report.phrase,
                ["paddlers"] = report.paddlers,
                ["photos"] = report.photos,
                ["weather"] = report.weather,
                ["author"] = report.author,
                ["coaches"] = report.coaches,
                ["id"] = report.id,
                ["published"] = true,
            });

            WriteAtomically(filePath, root.ToString(Formatting.Indented));
        }

        // The array of reports in a published reports file, and the root to write back (the array itself or its wrapper)
        private static JArray ReadItems(string json, string fileName, out JToken root)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                var empty = new JArray();
                root = empty;
                return empty;
            }

            try
            {
                root = JToken.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                throw new InvalidDataException($"{fileName} is not valid JSON (line {ex.LineNumber}, position {ex.LinePosition}): {ex.Message}", ex);
            }

            return root switch
            {
                JArray array => array,
                JObject obj when obj["items"] is JArray array => array,
                JObject obj => throw new InvalidDataException(
                    $"{fileName} must be an array of reports or an object with an \"items\" array; found an object with properties: {string.Join(", ", obj.Properties().Select(p => p.Name))}"),
                _ => throw new InvalidDataException($"{fileName} must be an array of reports; found {root.Type}"),
            };
        }

        private static List<PendingReport> ReadPending()
        {
            var filePath = Helper.MapPath(PendingFile);
            if (!File.Exists(filePath))
            {
                return new List<PendingReport>();
            }

            var json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<PendingReport>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<PendingReport>>(json) ?? new List<PendingReport>();
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"{PendingFile} could not be read: {ex.Message}", ex);
            }
        }

        private static void WritePending(List<PendingReport> all)
        {
            WriteAtomically(Helper.MapPath(PendingFile), JsonConvert.SerializeObject(all, Formatting.Indented));
        }

        // Write to a temporary file first, so a failure part way through never leaves a half-written data file
        private static void WriteAtomically(string filePath, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            var tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, content, new UTF8Encoding(false));
            File.Move(tempPath, filePath, overwrite: true);
        }

        private static string NewToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static bool TokenMatches(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(actual))
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(actual));
        }
    }
}
