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

app.MapControllers();

// Angular routes: anything that isn't an API call or a file falls back to the app's index.html
app.MapFallbackToFile("{*path:nonfile}", "index.html", staticFiles);
app.MapFallback("/api/{**path}", () => Results.NotFound());

app.Run();
