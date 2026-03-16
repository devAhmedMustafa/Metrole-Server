using Metrole.Modules.Auth;

namespace Metrole.Modules.Routing;

public class Route
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string OwnerId { get; set; }

    public User? Owner { get; set; }
}