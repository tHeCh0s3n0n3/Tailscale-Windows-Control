using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Tailscale_Windows_Control;

public sealed class TailscaleStatus
{
    public string? Version { get; set; }
    public string? BackendState { get; set; }
    public string? AuthURL { get; set; }
    public List<string>? TailscaleIPs { get; set; }
    public Peer? Self { get; set; }
    public object? Health { get; set; }
    public string? MagicDNSSuffix { get; set; }
    public CurrentTailnet? CurrentTailnet { get; set; }
    public object? CertDomains { get; set; }
    [JsonPropertyName("Peer")]
    public Dictionary<string, Peer>? Peers { get; set; }
    [JsonPropertyName("User")]
    public Dictionary<string, User>? Users { get; set; }

    public override string ToString()
    {
        return $"Status: {BackendState}, Peers: {Peers?.Count}, Users: {Users?.Count}";
    }
}

public sealed class User
{
    public long? ID { get; set; }
    public string? LoginName { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePicURL { get; set; }
    public List<object>? Roles { get; set; }

    public override string ToString()
    {
        return $"{DisplayName} ({LoginName}), Roles: {Roles?.Count}";
    }
}

public sealed class CurrentTailnet
{
    public string? Name { get; set; }
    public string? MagicDNSSuffix { get; set; }
    public bool? MagicDNSEnabled { get; set; }

    public override string ToString()
    {
        return $"{Name}, Enabled: {MagicDNSEnabled}, Suffix: {MagicDNSSuffix}";
    }
}

public sealed class Peer : IEquatable<Peer>
{
    public string? ID { get; set; }
    public string? PublicKey { get; set; }
    public string? HostName { get; set; }
    public string? DNSName { get; set; }
    public string? OS { get; set; }
    public long? UserID { get; set; }
    public List<string>? TailscaleIPs { get; set; }
    public List<string>? Addrs { get; set; }
    public string? CurAddr { get; set; }
    public string? Relay { get; set; }
    public int? RxBytes { get; set; }
    public int? TxBytes { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? LastWrite { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime? LastHandshake { get; set; }
    public bool? Online { get; set; }
    public bool? KeepAlive { get; set; }
    public bool? ExitNode { get; set; }
    public bool? ExitNodeOption { get; set; }
    public bool? Active { get; set; }
    public List<string>? PeerAPIURL { get; set; }
    public List<string>? Capabilities { get; set; }
    public bool? InNetworkMap { get; set; }
    public bool? InMagicSock { get; set; }
    public bool? InEngine { get; set; }

    public override string ToString()
    {
        return $"{HostName} ({Online}), Exit Node: {ExitNodeOption}";
    }

    public bool Equals(Peer? other)
    {
        if (other is null) return false;
        
        if (ReferenceEquals(this, other)) return true;

        if (other.GetType() != this.GetType()) return false;

        if (other is Peer peer)
        {
            return peer.ID == this.ID
                   && peer.Online == this.Online
                   && peer.Active == this.Active
                   && peer.ExitNode == this.ExitNode
                   && peer.ExitNodeOption == this.ExitNodeOption;
        }

        return this.Equals(other);
    }

    public override bool Equals(object? obj)
        => Equals(obj as Peer);

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = (int)2166136261;

            hash = (hash * 16777619) ^ ID?.GetHashCode() ?? string.Empty.GetHashCode();
            hash = (hash * 16777619) ^ Online.GetHashCode();
            hash = (hash * 16777619) ^ Active.GetHashCode();
            hash = (hash * 16777619) ^ ExitNode.GetHashCode();
            hash = (hash * 16777619) ^ ExitNodeOption.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(Peer p1, Peer p2)
        => p1.Equals(p2);

    public static bool operator !=(Peer p1, Peer p2)
        => !p1.Equals(p2);
}

public sealed class PeerComparer : IEqualityComparer<Peer>
{
    public bool Equals(Peer? x, Peer? y)
    {
        if (x is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode([DisallowNull] Peer obj)
    {
        if (obj is null) return 0;
        return obj.GetHashCode();
    }
}


