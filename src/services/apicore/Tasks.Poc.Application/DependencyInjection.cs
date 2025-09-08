namespace Tasks.Poc.Application;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tasks.Poc.Chassis.Mapper;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddMapper(typeof(IApiCoreMarker));

        return services;
    }
}
