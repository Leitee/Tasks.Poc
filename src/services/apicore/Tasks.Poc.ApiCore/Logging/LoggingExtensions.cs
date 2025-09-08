using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Tasks.Poc.ApiCore.Logging;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddSerilogWithOpenTelemetry(this WebApplicationBuilder builder)
    {
        BootstrapLogger.LogTransitionToRuntimeLogger();

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            var otelEndpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            var environment = context.HostingEnvironment.EnvironmentName;
            var applicationName = context.HostingEnvironment.ApplicationName;

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("Environment", environment)
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId();

            if (context.HostingEnvironment.IsDevelopment())
            {
                configuration
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.File(
                        formatter: new CompactJsonFormatter(),
                        path: Path.Combine("logs", "tasks-poc-.json"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        shared: true);
            }
            else
            {
                configuration
                    .WriteTo.Console(formatter: new CompactJsonFormatter())
                    .WriteTo.File(
                        formatter: new CompactJsonFormatter(),
                        path: Path.Combine("logs", "tasks-poc-.json"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        shared: true);
            }

            if (!string.IsNullOrWhiteSpace(otelEndpoint))
            {
                configuration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otelEndpoint;
                    options.ResourceAttributes.Add("service.name", applicationName);
                    options.ResourceAttributes.Add("service.version", GetApplicationVersion());
                    options.ResourceAttributes.Add("deployment.environment", environment);
                });
            }
        });

        return builder;
    }

    public static WebApplication UseEnhancedRequestLogging(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
        }
        else
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.GetLevel = GetLogLevel;
                options.EnrichDiagnosticContext = EnrichFromRequest;
            });
        }

        return app;
    }

    private static LogEventLevel GetLogLevel(HttpContext ctx, double elapsedMs, Exception? ex) =>
        ex != null
            ? LogEventLevel.Error
            : ctx.Response.StatusCode > 499
                ? LogEventLevel.Error
                : ctx.Response.StatusCode > 399
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

    private static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme ?? "unknown");
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown");
        diagnosticContext.Set("ClientIP", GetClientIpAddress(httpContext) ?? "unknown");
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
            diagnosticContext.Set("UserName", httpContext.User.Identity.Name ?? "unknown");
        }

        if (httpContext.Request.ContentLength.HasValue && httpContext.Request.ContentLength > 0)
        {
            diagnosticContext.Set("RequestContentLength", httpContext.Request.ContentLength.Value);
        }

        if (httpContext.Response.ContentLength.HasValue)
        {
            diagnosticContext.Set("ResponseContentLength", httpContext.Response.ContentLength.Value);
        }
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',').First().Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static string GetApplicationVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }
}
