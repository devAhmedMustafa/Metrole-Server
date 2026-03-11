using Metrole.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metrole.Modules.Auth;

public class AuthDbConfig : IDbConfig<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).IsRequired();
        builder.Property(x => x.Password).IsRequired();
    }
}