using System;
using Datadog.Trace;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Compact;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Initialize Serilog
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .Enrich.FromLogContext()
    // For JSON formatted logs in the console, uncomment the next line:
    // .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "/tmp/logs/myapp.txt", rollingInterval: RollingInterval.Day));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware for adding Datadog trace information to Serilog's LogContext for each request
app.Use(async (context, next) =>
{
    using (LogContext.PushProperty("dd_env", CorrelationIdentifier.Env))
    using (LogContext.PushProperty("dd_service", CorrelationIdentifier.Service))
    using (LogContext.PushProperty("dd_version", CorrelationIdentifier.Version))
    using (LogContext.PushProperty("dd_trace_id", CorrelationIdentifier.TraceId.ToString()))
    using (LogContext.PushProperty("dd_span_id", CorrelationIdentifier.SpanId.ToString()))
    {
        await next();
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/namevalidation", (string name) =>
{
    // Validate that the name contains only letters
    if (string.IsNullOrWhiteSpace(name))
    {
        Log.Warning("Name is required for name validation.");
        return Results.BadRequest("Name is required.");
    }
    if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z]+$"))
    {
        Log.Warning("Invalid characters in name. Only letters are allowed.");
        return Results.BadRequest("Invalid characters in name. Only letters are allowed.");
    }

    Log.Information("Name validation successful for: {Name}", name);
    return Results.Ok($"Hello, {name}!");
})
.WithName("GetNameValidation")
.WithOpenApi();

app.Run();
