using MediatR;
using Serilog.Context;
using System.Diagnostics;
using System.Text.Json;

namespace Tasks.Poc.ApiCore.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    private static bool IsPrimitiveType(Type type)
    {
        return type.IsPrimitive || 
               type == typeof(string) || 
               type == typeof(DateTime) || 
               type == typeof(DateTimeOffset) || 
               type == typeof(TimeSpan) || 
               type == typeof(Guid) || 
               type == typeof(decimal) || 
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                IsPrimitiveType(Nullable.GetUnderlyingType(type)!));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("RequestType", requestName))
        using (LogContext.PushProperty("MediatRRequestId", requestId))
        {
            _logger.LogInformation("Starting {RequestName} with ID {RequestId}", requestName, requestId);

            try
            {
                LogRequestDetails(request, requestName, requestId);

                var response = await next();
                
                stopwatch.Stop();
                
                LogSuccessResponse(requestName, requestId, response, stopwatch.Elapsed);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogException(requestName, requestId, ex, stopwatch.Elapsed);
                throw;
            }
        }
    }

    private void LogRequestDetails(TRequest request, string requestName, string requestId)
    {
        try
        {
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            _logger.LogDebug("Request {RequestName} {RequestId} details: {RequestData}", 
                requestName, requestId, requestJson);

            LogDomainSpecificDetails(request, requestName, requestId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize request {RequestName} {RequestId} for logging", 
                requestName, requestId);
        }
    }

    private void LogDomainSpecificDetails(TRequest request, string requestName, string requestId)
    {
        var requestType = request.GetType();

        var userIdProperty = requestType.GetProperty("UserId");
        if (userIdProperty != null && userIdProperty.GetValue(request) is Guid userId && userId != Guid.Empty)
        {
            _logger.LogDebug("Request {RequestName} {RequestId} involves user {UserId}", 
                requestName, requestId, userId);
        }

        var todoListIdProperty = requestType.GetProperty("TodoListId");
        if (todoListIdProperty != null && todoListIdProperty.GetValue(request) is Guid todoListId && todoListId != Guid.Empty)
        {
            _logger.LogDebug("Request {RequestName} {RequestId} involves todo list {TodoListId}", 
                requestName, requestId, todoListId);
        }

        var itemIdProperty = requestType.GetProperty("ItemId");
        if (itemIdProperty != null && itemIdProperty.GetValue(request) is Guid itemId && itemId != Guid.Empty)
        {
            _logger.LogDebug("Request {RequestName} {RequestId} involves todo item {ItemId}", 
                requestName, requestId, itemId);
        }

        var emailProperty = requestType.GetProperty("Email");
        if (emailProperty != null && emailProperty.GetValue(request) is string email && !string.IsNullOrEmpty(email))
        {
            _logger.LogDebug("Request {RequestName} {RequestId} involves email {Email}", 
                requestName, requestId, email);
        }
    }

    private void LogSuccessResponse(string requestName, string requestId, TResponse response, TimeSpan elapsed)
    {
        var elapsedMs = elapsed.TotalMilliseconds;
        
        _logger.LogInformation("Completed {RequestName} {RequestId} in {ElapsedMs:0.0000}ms", 
            requestName, requestId, elapsedMs);

        if (elapsedMs > 5000) // Slow operation threshold
        {
            _logger.LogWarning("Slow operation detected: {RequestName} {RequestId} took {ElapsedMs:0.0000}ms", 
                requestName, requestId, elapsedMs);
        }
        else if (elapsedMs > 1000) // Warning threshold
        {
            _logger.LogInformation("Operation performance warning: {RequestName} {RequestId} took {ElapsedMs:0.0000}ms", 
                requestName, requestId, elapsedMs);
        }

        try
        {
            LogResponseDetails(response, requestName, requestId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize response for {RequestName} {RequestId}", 
                requestName, requestId);
        }
    }

    private void LogResponseDetails(TResponse response, string requestName, string requestId)
    {
        if (response == null)
        {
            _logger.LogDebug("Response for {RequestName} {RequestId} is null", requestName, requestId);
            return;
        }

        var responseType = response.GetType();

        if (responseType == typeof(Guid) && response is Guid guidResponse && guidResponse != Guid.Empty)
        {
            _logger.LogDebug("Response {RequestName} {RequestId} created entity with ID {EntityId}", 
                requestName, requestId, guidResponse);
        }
        else if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var listResponse = response as System.Collections.IList;
            var count = listResponse?.Count ?? 0;
            _logger.LogDebug("Response {RequestName} {RequestId} returned {ItemCount} items", 
                requestName, requestId, count);
        }
        else if (!IsPrimitiveType(responseType))
        {
            var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
            
            _logger.LogDebug("Response {RequestName} {RequestId} details: {ResponseData}", 
                requestName, requestId, responseJson);
        }
    }

    private void LogException(string requestName, string requestId, Exception ex, TimeSpan elapsed)
    {
        _logger.LogError(ex, "Request {RequestName} {RequestId} failed after {ElapsedMs:0.0000}ms with exception {ExceptionType}: {ExceptionMessage}",
            requestName, requestId, elapsed.TotalMilliseconds, ex.GetType().Name, ex.Message);

        if (ex.InnerException != null)
        {
            _logger.LogError("Inner exception for {RequestName} {RequestId}: {InnerExceptionType}: {InnerExceptionMessage}",
                requestName, requestId, ex.InnerException.GetType().Name, ex.InnerException.Message);
        }
    }
}
