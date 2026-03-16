namespace Metrole.Modules.Tunneling;

public class Tunnel
{
    public string Subdomain { get; }
    public string Host { get; }
    public int Port { get; }
    public DateTimeOffset RegisteredAt { get; }

    public Tunnel(string subdomain, string host, int port)
    {
        Subdomain = subdomain;
        Host = host;
        Port = port;
        RegisteredAt = DateTimeOffset.UtcNow;
    }
}