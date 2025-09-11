using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.Enrichment;
using Tasks.Poc.Contracts.Constants.Other;

namespace Tasks.Poc.Chassis.Logging;

public sealed class ApplicationEnricher(IHttpContextAccessor httpContextAccessor) : ILogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector)
    {
        collector.Add(LoggingConstant.MachineName, Environment.MachineName);

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            collector.Add(
                LoggingConstant.UserId,
                httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            );
        }
    }
}
