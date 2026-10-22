using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Serialization;
using sckc.api.Extensions;
using System;
using System.IO;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        // Web API 2 serialised string results as JSON, so keep that behaviour rather than returning text/plain
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
    })
    .AddNewtonsoftJson(options =>
    {
        // Keep the PascalCase property names that the Angular app expects
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

Helper.ContentRootPath = app.Environment.ContentRootPath;

// Unhandled errors are returned as problem details JSON (exception type and message, no stack trace) so a failing API
// call shows what went wrong in the browser's network tab. The full exception is also appended to logs/errors.log,
// which can be downloaded over FTP but is never served (see blockedFolders below).
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
    var traceId = context.TraceIdentifier;
    var request = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";

    app.Logger.LogError(exception, "Unhandled exception for {Request} (trace {TraceId})", request, traceId);
    try
    {
        var logFolder = Path.Combine(app.Environment.ContentRootPath, "logs");
        Directory.CreateDirectory(logFolder);
        await File.AppendAllTextAsync(Path.Combine(logFolder, "errors.log"),
            $"{DateTime.UtcNow:u} {request} trace={traceId}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
    }
    catch (Exception logException)
    {
        app.Logger.LogError(logException, "Could not write to logs/errors.log");
    }

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new
    {
        title = "The server could not complete the request",
        status = StatusCodes.Status500InternalServerError,
        detail = exception?.Message,
        exception = exception?.GetType().Name,
        traceId,
    }, (System.Text.Json.JsonSerializerOptions)null, "application/problem+json");
}));

if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseSwagger();
app.UseSwaggerUI();

// The Angular app, gallery images and API are all deployed to the same folder, so static files are served from
// the content root. Block anything that isn't public content (secrets, server binaries, config).
string[] blockedFolders = ["/data", "/Resources", "/bin", "/logs"];
string[] blockedExtensions = [".dll", ".pdb", ".exe", ".config", ".data"];
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var fileName = Path.GetFileName(path.Value ?? string.Empty);
    var blocked = blockedFolders.Any(folder => path.StartsWithSegments(folder, StringComparison.OrdinalIgnoreCase))
        || blockedExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase)
        || fileName.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase)
        || fileName.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase)
        || fileName.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase);

    if (blocked)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next();
});

var staticFiles = new StaticFileOptions { FileProvider = app.Environment.ContentRootFileProvider };
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = app.Environment.ContentRootFileProvider });
app.UseStaticFiles(staticFiles);

// Routing has to come after the static file middleware, otherwise the fallback endpoints below are chosen before
// UseDefaultFiles can map a folder request such as /app/ to its index.html
app.UseRouting();

app.MapControllers();

// Angular routes: anything that isn't an API call or a file falls back to the app's index.html. The staged build is
// deployed under app/ (and uses this API), so its routes fall back to app/index.html.
app.MapFallbackToFile("{*path:nonfile}", "index.html", staticFiles);
app.MapFallbackToFile("app/{*path:nonfile}", "app/index.html", staticFiles);
app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallback("/app/api/{**path}", () => Results.NotFound());

app.Run();

// Exposes the entry point to the integration tests (WebApplicationFactory<Program>)
public partial class Program { }
