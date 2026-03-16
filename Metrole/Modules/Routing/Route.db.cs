using Metrole.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metrole.Modules.Routing;

public class RouteDbConfig : IDbConfig<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.HasKey(x => x.Name);
        builder.Property(x => x.Url).IsRequired();
        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}