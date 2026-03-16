using System.Net;
using System.Net.Sockets;
using System.Text;
using Metrole.Modules.Routing;

namespace Metrole.Modules.Tunneling;

public class TcpRouter : BackgroundService
{
    private readonly TcpListener _listener = new (IPAddress.Any, 24125);
    private readonly TunnelRegistry _tunnelRegistry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TcpRouter> _logger;

    public TcpRouter(TunnelRegistry tunnelRegistry, IServiceScopeFactory scopeFactory, ILogger<TcpRouter> logger)
    {
        _tunnelRegistry = tunnelRegistry;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _listener.Start();
        _logger.LogInformation("TCP Router started on port {Port}", ((IPEndPoint)_listener.LocalEndpoint).Port);

        while (!ct.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(ct);
            _logger.LogInformation("Accepted TCP connection from {RemoteEndPoint}", client.Client.RemoteEndPoint);
            _ = HandleClientAsync(client, ct);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];

            var bytesRead = await stream.ReadAsync(buffer, ct);
            if (bytesRead == 0)
            {
                client.Close();
                return;
            }

            var header = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var subdomain = ParseSubdomain(header);
            if (subdomain == null)
            {
                _logger.LogWarning("No Host header found in TCP request");
                client.Close();
                return;
            }

            var endpoint = await ResolveEndpointAsync(subdomain);
            if (endpoint == null)
            {
                _logger.LogWarning("No routing target found for subdomain {Subdomain}", subdomain);
                client.Close();
                return;
            }

            var daemon = new TcpClient();
            await daemon.ConnectAsync(endpoint.Value.host, endpoint.Value.port, ct);

            var daemonStream = daemon.GetStream();
            await daemonStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);

            var upstream = stream.CopyToAsync(daemonStream, ct);
            var downstream = daemonStream.CopyToAsync(stream, ct);
            await Task.WhenAny(upstream, downstream);

            client.Close();
            daemon.Close();
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling TCP client");
            client.Close();
        }
    }

    private async Task<(string host, int port)?> ResolveEndpointAsync(string subdomain)
    {
        var liveTunnel = _tunnelRegistry.GetTunnel(subdomain);
        if (liveTunnel != null)
        {
            return (liveTunnel.Host, liveTunnel.Port);
        }

        using var scope = _scopeFactory.CreateScope();
        var routeService = scope.ServiceProvider.GetService<RouteService>();
        if (routeService == null)
        {
            return null;
        }

        var route = await routeService.ResolveIp(subdomain);
        if (route == null || string.IsNullOrWhiteSpace(route.Url))
        {
            return null;
        }

        if (!TryParseEndpoint(route.Url, out var host, out var port))
        {
            return null;
        }

        return (host, port);
    }

    private static bool TryParseEndpoint(string value, out string host, out int port)
    {
        host = string.Empty;
        port = 0;

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host) && uri.Port > 0)
        {
            host = uri.Host;
            port = uri.Port;
            return true;
        }

        var lastColon = value.LastIndexOf(':');
        if (lastColon <= 0)
        {
            return false;
        }

        host = value[..lastColon].Trim();
        if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            host = host["http://".Length..];
        }
        else if (host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            host = host["https://".Length..];
        }

        return int.TryParse(value[(lastColon + 1)..], out port) && port > 0;
    }

    private static string? ParseSubdomain(string headers)
    {
        var hostLine = headers.Split("\r\n").FirstOrDefault(l => l.StartsWith("Host:", StringComparison.OrdinalIgnoreCase));
        if (hostLine == null) return null;

        var host = hostLine.Split(':', 2)[1].Trim();
        var subdomain = host.Split('.')[0];
        return subdomain;
    }
}