using Metrole.Core;

namespace Metrole.Modules.Tunneling;

public class TunnelModule : IModule
{
    public void Configure(WebApplication app)
    {
        app.MapGrpcService<TunnelService>();
    }

    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddSingleton<TunnelRegistry>();
        services.AddHostedService<TcpRouter>();
    }
}
