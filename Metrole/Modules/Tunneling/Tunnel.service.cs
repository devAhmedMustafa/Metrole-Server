using System.Net;
using Grpc.Core;
using Metrole.Tunneling;

namespace Metrole.Modules.Tunneling;

public class TunnelService : TunnelGrpc.TunnelGrpcBase
{
    private readonly TunnelRegistry _tunnelRegistry;
    private readonly ILogger<TunnelService> _logger;
    
    public TunnelService(TunnelRegistry tunnelRegistry, ILogger<TunnelService> logger)
    {
        _tunnelRegistry = tunnelRegistry;
        _logger = logger;
    }

    public override async Task<TunnelMessage> OpenTunnel(
        OpenTunnelRequest request,
        ServerCallContext context
    )
    {
        if (string.IsNullOrWhiteSpace(request.Subdomain))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Subdomain is required."));
        }

        if (request.Port <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Port must be greater than 0."));
        }

        var host = ParsePeerHost(context.Peer);
        var tunnel = new Tunnel(request.Subdomain, host, request.Port);
        _tunnelRegistry.RegisterTunnel(request.Subdomain, tunnel);

        _logger.LogInformation(
            "Tunnel registered for subdomain {Subdomain} at {Host}:{Port}",
            request.Subdomain,
            host,
            request.Port
        );

        var message = new TunnelMessage
        {
            Type = TunnelMessage.Types.Type.Ack,
            Payload = $"Tunnel registered for {request.Subdomain} at {host}:{request.Port}"
        };

        return message;
    }

    private static string ParsePeerHost(string peer)
    {
        if (string.IsNullOrWhiteSpace(peer))
        {
            return "127.0.0.1";
        }

        var value = peer;
        var schemeSeparatorIndex = value.IndexOf(':');
        if (schemeSeparatorIndex >= 0)
        {
            value = value[(schemeSeparatorIndex + 1)..];
        }

        if (value.StartsWith("[", StringComparison.Ordinal))
        {
            var endBracket = value.IndexOf(']');
            if (endBracket > 1)
            {
                return NormalizeIp(value[1..endBracket]);
            }
        }

        var lastColon = value.LastIndexOf(':');
        if (lastColon > 0)
        {
            return NormalizeIp(value[..lastColon]);
        }

        return NormalizeIp(value);
    }

    private static string NormalizeIp(string host)
    {
        if (IPAddress.TryParse(host, out var ip))
        {
            if (ip.IsIPv4MappedToIPv6) return ip.MapToIPv4().ToString();
            if (ip.Equals(IPAddress.IPv6Loopback)) return IPAddress.Loopback.ToString();
        }

        return host;
    }
}