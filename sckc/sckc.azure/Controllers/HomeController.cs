using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using sckc.azure.Extensions;
using sckc.azure.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace sckc.azure.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            if (Request.IsAuthenticated || true)
            {
                return View(await this.getClubEvents());
            }
            return View();
        }

        public async Task<ActionResult> Bookings(string Id)
        {
            if (Request.IsAuthenticated || true)
            {
                if (string.IsNullOrWhiteSpace(Id))
                {
                    return RedirectToAction("Index");
                }

                return View(await getBookings(Id));
            }
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<Booking>> getBookings(string Id)
        {
            var connectionString = System.IO.File.ReadAllText(Helper.MapPath("data/azure_storage_connstring.data")).Trim();

            var client = new TableClient(connectionString, "bookings");

            var table = await client.CreateIfNotExistsAsync();

            Pageable<TableEntity> queryResultsFilter = client.Query<TableEntity>(filter: $"PartitionKey eq '{Id}'");

            List<Booking> bookings = new List<Booking>();
            foreach (var booking in queryResultsFilter)
            {
                bookings.Add(new Booking() { 
                    Name = booking.GetString("Name"),
                    Email = booking.RowKey,
                    BookingType = booking.GetString("BookingType"),
                    IsLiveBooking = "Live".Equals(booking.GetString("Type"), StringComparison.InvariantCultureIgnoreCase),
                    People = booking.GetInt32("People"),
                    Amount = booking["Amount"] != null ? (double?)Convert.ToDouble(booking["Amount"].ToString()) : null,
                });
            }

            return bookings;
        }

        private async Task<IEnumerable<ClubEvent>> getClubEvents()
        {
            string endpoint = "http://www.sheffieldcitykayakclub.co.uk/app/api/GetEvents?count=20";

            using (HttpClient client = new HttpClient())
            {
                using (var Response = await client.GetAsync(endpoint))
                {
                    if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<ClubEvent>>(await Response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        throw new Exception("Failed to load Events");
                    }
                }

            }
        }
    }
}