using Metrole.Core;
using Metrole.Data;
using Metrole.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MetroleDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddGrpc();

var modules = typeof(Program).Assembly.GetTypes()
	.Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
	.Select(t => (IModule)Activator.CreateInstance(t)!);

foreach (var module in modules)
{
    module.Register(builder.Services, builder.Configuration, builder.Environment);
}

var app = builder.Build();

foreach (var module in modules)
{
	module.Configure(app);
}

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();

app.Run();
