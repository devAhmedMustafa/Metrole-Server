namespace Metrole.Modules.Routing.Dtos;

public class AssignRouteRequest
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string UserId { get; set; }
}