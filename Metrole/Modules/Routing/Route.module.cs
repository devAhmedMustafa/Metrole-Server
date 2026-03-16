using Metrole.Core;

namespace Metrole.Modules.Routing;

public class RouteModule : IModule
{
    public void Configure(WebApplication app)
    {
        app.MapGrpcService<RouteRpc>();
    }

    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<RouteService>();
    }
}