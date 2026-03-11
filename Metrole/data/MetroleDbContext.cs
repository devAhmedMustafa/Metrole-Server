namespace Metrole.Data;

using Microsoft.EntityFrameworkCore;

public class MetroleDbContext : DbContext
{
    public DbSet<Modules.Auth.User> Users { get; set; }

    public MetroleDbContext(DbContextOptions<MetroleDbContext> options) : base(options)
    {
    }
}