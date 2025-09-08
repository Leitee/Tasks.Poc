using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Tasks.Poc.ApiCore.Logging;

public static class BootstrapLogger
{
    private static readonly string LogsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
    private static bool _isInitialized = false;

    public static void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        EnsureLogsDirectoryExists();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.WithProperty("Application", "Tasks.Poc.ApiCore")
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] [Bootstrap] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                formatter: new CompactJsonFormatter(),
                path: Path.Combine(LogsDirectory, "bootstrap-.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true)
            .CreateBootstrapLogger();

        _isInitialized = true;

        Log.Information("Bootstrap logger initialized for application startup");
        Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        Log.Information("Base directory: {BaseDirectory}", AppContext.BaseDirectory);
        Log.Information("Logs directory: {LogsDirectory}", LogsDirectory);
    }

    public static void LogStartupMetrics(TimeSpan startupDuration, string phase)
    {
        Log.Information("Startup phase {Phase} completed in {StartupDuration}ms", 
            phase, startupDuration.TotalMilliseconds);
    }

    public static void LogConfigurationLoaded(IConfiguration configuration)
    {
        var connectionStringsCount = configuration.GetSection("ConnectionStrings").GetChildren().Count();
        var loggingConfig = configuration.GetSection("Serilog").Exists();
        var otelConfig = configuration.GetSection("OpenTelemetry").Exists();

        Log.Information("Configuration loaded - ConnectionStrings: {ConnectionStringsCount}, Serilog: {HasSerilogConfig}, OpenTelemetry: {HasOtelConfig}",
            connectionStringsCount, loggingConfig, otelConfig);
    }

    public static void LogDatabaseConnectionAttempt(string connectionName, string database)
    {
        Log.Information("Attempting database connection {ConnectionName} to {Database}", 
            connectionName, database);
    }

    public static void LogDatabaseConnectionSuccess(string connectionName, TimeSpan duration)
    {
        Log.Information("Database connection {ConnectionName} successful in {Duration}ms", 
            connectionName, duration.TotalMilliseconds);
    }

    public static void LogDatabaseConnectionFailure(string connectionName, Exception exception)
    {
        Log.Error(exception, "Database connection {ConnectionName} failed", connectionName);
    }

    public static void LogTransitionToRuntimeLogger()
    {
        Log.Information("Transitioning from bootstrap logger to runtime logger with full OpenTelemetry integration");
    }

    public static void Flush()
    {
        Log.CloseAndFlush();
    }

    private static void EnsureLogsDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(LogsDirectory))
            {
                Directory.CreateDirectory(LogsDirectory);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create logs directory: {ex.Message}");
        }
    }
}