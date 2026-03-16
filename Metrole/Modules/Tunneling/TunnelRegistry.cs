using System.Collections.Concurrent;

namespace Metrole.Modules.Tunneling;

public class TunnelRegistry
{
    private readonly ConcurrentDictionary<string, Tunnel> _tunnels = new();

    public void RegisterTunnel(string routeName, Tunnel tunnel)
    {
        _tunnels[routeName] = tunnel;
    }

    public Tunnel? GetTunnel(string routeName)
    {
        _tunnels.TryGetValue(routeName, out var tunnel);
        return tunnel;
    }

    public bool RemoveTunnel(string routeName)
    {
        return _tunnels.TryRemove(routeName, out _);
    }

    public IReadOnlyDictionary<string, Tunnel> GetAll()
    {
        return _tunnels;
    }
}