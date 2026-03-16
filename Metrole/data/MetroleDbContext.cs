namespace Metrole.Data;

using Microsoft.EntityFrameworkCore;

public class MetroleDbContext : DbContext
{
    public DbSet<Modules.Auth.User> Users { get; set; }
    public DbSet<Modules.Routing.Route> Routes { get; set; }

    public MetroleDbContext(DbContextOptions<MetroleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetroleDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}