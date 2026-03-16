using System.Security.Claims;
using Grpc.Core;
using Metrole.Modules.Routing.Dtos;
using Microsoft.AspNetCore.Authorization;
using Routing;

namespace Metrole.Modules.Routing;

[Authorize]
public class RouteRpc : RouteGrpc.RouteGrpcBase
{
    private readonly RouteService _routeService;

    public RouteRpc(RouteService routeService)
    {
        _routeService = routeService;
    }

    public override async Task<RegisterRouteResponse> RegisterRoute(RegisterRouteRequest request, ServerCallContext context)
    {
        try
        {
            var httpContext = context.GetHttpContext();
            if (httpContext == null)
            {
                return new RegisterRouteResponse
                {
                    Success = false,
                    Message = "Invalid HTTP context."
                };
            }

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");

            var reqDto = new AssignRouteRequest
            {
                UserId = userId,
                Name = request.Name,
                Url = request.Url
            };

            await _routeService.RegisterRoute(reqDto);

            return new RegisterRouteResponse
            {
                Success = true,
                Message = "Route registered successfully."
            };
        }
        catch (Exception ex)
        {
            return new RegisterRouteResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }
}