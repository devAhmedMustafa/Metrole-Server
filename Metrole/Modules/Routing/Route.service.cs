using System.Collections.Concurrent;
using Metrole.Data;
using Metrole.Modules.Routing.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Metrole.Modules.Routing;

public class RouteService
{
    private readonly MetroleDbContext _db;
    private readonly ConcurrentDictionary<string, Route> _routeCache = new();

    public RouteService(MetroleDbContext db)
    {
        _db = db;
    }

    public async Task RegisterRoute(AssignRouteRequest request)
    {
        try
        {
            var route = new Route
            {
                Name = request.Name,
                Url = request.Url,
                OwnerId = request.UserId
            };

            _db.Routes.Add(route);
            await _db.SaveChangesAsync();

            _routeCache[route.Name] = route;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task UpdateIp(string subdomain, string newIp)
    {
        try {
            var route = await _db.Routes.FirstOrDefaultAsync(r => r.Name == subdomain);
            if (route == null)
            {
                throw new Exception("Route not found");
            }

            route.Url = newIp;
            await _db.SaveChangesAsync();

            _routeCache[subdomain] = route;
        }
        catch(Exception){
            throw;
        }
    }

    public async Task<Route?> ResolveIp(string subdomain)
    {
        if (_routeCache.TryGetValue(subdomain, out var cachedRoute))
        {
            return cachedRoute;
        }

        var route = await _db.Routes.FirstOrDefaultAsync(r => r.Name == subdomain);
        if (route != null)
        {
            _routeCache[subdomain] = route;
        }

        return route;
    }
    
}