using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Text;

namespace Tasks.Poc.ApiCore.Logging;

public class RequestLoggingMiddleware
{
    private static readonly string[] SensitiveHeaders = { "authorization", "cookie", "x-api-key" };
    private static readonly string[] HealthCheckPaths = { "/health", "/alive", "/ready" };

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    private static bool IsHealthCheck(string path)
    {
        return HealthCheckPaths.Any(healthPath => 
            string.Equals(path, healthPath, StringComparison.OrdinalIgnoreCase));
    }

    private static LogLevel GetLogLevel(int statusCode) => statusCode switch
    {
        >= 500 => LogLevel.Error,
        >= 400 => LogLevel.Warning,
        _ => LogLevel.Information
    };

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string correlationIdHeader = "X-Correlation-ID";
        
        if (context.Request.Headers.TryGetValue(correlationIdHeader, out var correlationId) && 
            !string.IsNullOrWhiteSpace(correlationId))
        {
            context.Response.Headers.TryAdd(correlationIdHeader, correlationId);
            return correlationId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Response.Headers.TryAdd(correlationIdHeader, newCorrelationId);
        return newCorrelationId;
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

    private static Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        return headers
            .Where(h => !SensitiveHeaders.Contains(h.Key.ToLowerInvariant()))
            .ToDictionary(h => h.Key, h => string.Join(", ", h.Value.AsEnumerable()));
    }

    private static Dictionary<string, string> GetSafeHeaders(Dictionary<string, Microsoft.Extensions.Primitives.StringValues> headers)
    {
        return headers
            .Where(h => !SensitiveHeaders.Contains(h.Key.ToLowerInvariant()))
            .ToDictionary(h => h.Key, h => string.Join(", ", h.Value.AsEnumerable()));
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        if (request.ContentLength == 0 || !request.ContentLength.HasValue)
        {
            return false;
        }

        var contentType = request.ContentType?.ToLowerInvariant();
        return contentType != null && (
            contentType.Contains("application/json") ||
            contentType.Contains("application/xml") ||
            contentType.Contains("text/"));
    }

    private static bool ShouldLogResponseBody(HttpResponse response)
    {
        if (response.ContentLength == 0)
        {
            return false;
        }

        var contentType = response.ContentType?.ToLowerInvariant();
        return contentType != null && (
            contentType.Contains("application/json") ||
            contentType.Contains("application/xml") ||
            contentType.Contains("text/"));
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        
        return body;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsHealthCheck(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        var requestId = context.TraceIdentifier;
        var correlationId = GetOrCreateCorrelationId(context);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", requestId))
        {
            try
            {
                await LogRequestAsync(context, request);

                var originalResponseBodyStream = context.Response.Body;
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                await _next(context);

                stopwatch.Stop();

                await LogResponseAsync(context, stopwatch.Elapsed, responseBodyStream);

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogException(context, ex, stopwatch.Elapsed);
                throw;
            }
        }
    }

    private async Task LogRequestAsync(HttpContext context, HttpRequest request)
    {
        var requestInfo = new
        {
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            Headers = GetSafeHeaders(request.Headers),
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            Host = request.Host.Value,
            Scheme = request.Scheme,
            ClientIP = GetClientIpAddress(context),
            UserAgent = request.Headers.UserAgent.FirstOrDefault()
        };

        _logger.LogInformation("HTTP Request {Method} {Path}{QueryString} started - {RequestInfo}", 
            request.Method, request.Path, request.QueryString, requestInfo);

        if (ShouldLogRequestBody(request))
        {
            var requestBody = await ReadRequestBodyAsync(request);
            if (!string.IsNullOrEmpty(requestBody))
            {
                _logger.LogDebug("Request body: {RequestBody}", requestBody);
            }
        }
    }

    private async Task LogResponseAsync(HttpContext context, TimeSpan duration, MemoryStream responseBodyStream)
    {
        var response = context.Response;
        var responseInfo = new
        {
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            ContentLength = response.ContentLength ?? responseBodyStream.Length,
            Headers = GetSafeHeaders(response.Headers.ToDictionary(h => h.Key, h => h.Value))
        };

        var logLevel = GetLogLevel(response.StatusCode);
        _logger.Log(logLevel, "HTTP Response {Method} {Path} responded {StatusCode} in {Duration:0.0000}ms - {ResponseInfo}",
            context.Request.Method, context.Request.Path, response.StatusCode, duration.TotalMilliseconds, responseInfo);

        if (ShouldLogResponseBody(response) && responseBodyStream.Length > 0)
        {
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            _logger.LogDebug("Response body: {ResponseBody}", responseBody);
        }

        LogPerformanceMetrics(context, duration);
    }

    private void LogException(HttpContext context, Exception ex, TimeSpan duration)
    {
        _logger.LogError(ex, "HTTP Request {Method} {Path} failed after {Duration:0.0000}ms with exception: {ExceptionType}",
            context.Request.Method, context.Request.Path, duration.TotalMilliseconds, ex.GetType().Name);
    }

    private void LogPerformanceMetrics(HttpContext context, TimeSpan duration)
    {
        var durationMs = duration.TotalMilliseconds;
        
        if (durationMs > 5000) // Slow request threshold
        {
            _logger.LogWarning("Slow request detected: {Method} {Path} took {Duration:0.0000}ms", 
                context.Request.Method, context.Request.Path, durationMs);
        }
        else if (durationMs > 1000) // Warning threshold
        {
            _logger.LogInformation("Request performance warning: {Method} {Path} took {Duration:0.0000}ms", 
                context.Request.Method, context.Request.Path, durationMs);
        }
    }
}
