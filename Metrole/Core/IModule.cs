namespace Metrole.Core;

public interface IModule
{
    abstract void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    );

    abstract void Configure(
        WebApplication app
    );

}