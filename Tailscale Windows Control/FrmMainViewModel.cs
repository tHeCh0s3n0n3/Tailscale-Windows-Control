using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Reflection;

namespace Tailscale_Windows_Control;

public sealed partial class FrmMainViewModel : ObservableObject
{
    private static readonly Assembly AppContext = Assembly.GetExecutingAssembly();
    //private static readonly string[] Resources = AppContext.GetManifestResourceNames();
    private static readonly Icon ConnectedIcon = new(AppContext.GetManifestResourceStream("Tailscale_Windows_Control.Icons.Tailscale_connected.ico") ?? Stream.Null);
    private static readonly Icon DisconnectedIcon = new(AppContext.GetManifestResourceStream("Tailscale_Windows_Control.Icons.Tailscale.ico") ?? Stream.Null);
    private static readonly Icon ConnectedIconOverlay = new(AppContext.GetManifestResourceStream("Tailscale_Windows_Control.Icons.Tailscale_connected_arrow.ico") ?? Stream.Null);

    public string TailscaleLocation { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskbarIconOverlayText))]
    private string? statusLabel;

    [ObservableProperty]
    private string? commandResult;

    [ObservableProperty]
    private TailscaleStatus? status;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskbarIcon))]
    [NotifyPropertyChangedFor(nameof(TaskbarIconOverlay))]
    [NotifyPropertyChangedFor(nameof(TaskbarIconOverlayText))]
    private bool isConnected;

    public Icon TaskbarIcon
        => IsConnected ? ConnectedIcon : DisconnectedIcon;

    public Icon? TaskbarIconOverlay
        => IsConnected ? ConnectedIconOverlay : null;

    public string? TaskbarIconOverlayText
        => IsConnected ? StatusLabel : null;

    //public List<Peer>? ExitNodePeers
    //    => status?.Peers?.Select(p => p.Value)
    //                     .Where(p => p.ExitNodeOption.HasValue
    //                                 && p.ExitNodeOption.Value)
    //                     .ToList();

    public FrmMainViewModel(string tailscaleExecutablePath)
    {
        TailscaleLocation = tailscaleExecutablePath;
    }

    private async Task Connect(IPAddress? ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        CommandResult = await ExecuteTailscaleCommand($"up --reset --exit-node={ipAddress} --exit-node-allow-lan-access");

        StatusLabel = "Connecting...";
    }

    private async Task Disconnect()
    {
        CommandResult = await ExecuteTailscaleCommand("up --reset --exit-node=");
        StatusLabel = "Disconnecting...";
    }

    [RelayCommand]
    private async Task<string> ExecuteTailscaleCommand(string arguments)
    {
        using Process p = new();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = TailscaleLocation;
        p.StartInfo.Arguments = arguments;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.LoadUserProfile = false;
        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        p.Start();

        string output = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();

        return output;
    }

    [RelayCommand]
    private async Task GetStatus()
    {
        string output = await ExecuteTailscaleCommand("status --json");
        output = output.Replace("\r\n", "\n")
                       .Replace("\n\r", "\n")
                       .Replace("\r", "\n")
                       .Replace("\n", Environment.NewLine);
        if (CommandResult is null
            || !CommandResult.Equals(output))
        {
            CommandResult = output;
        }

        Status = await JsonSerializer.DeserializeAsync<TailscaleStatus>(new MemoryStream(Encoding.UTF8.GetBytes(output)));
        
        Peer? connectedPeer = Status?.Peers
                                    ?.Select(p => p.Value)
                                    ?.FirstOrDefault(p => p.ExitNode.HasValue
                                                          && p.ExitNode.Value);
        if (connectedPeer is not null)
        {
            StatusLabel = $"Connected ({connectedPeer.HostName})";
            IsConnected = true;
        }
        else
        {
            StatusLabel = "Ready!";
            IsConnected = false;
        }
    }

    [RelayCommand]
    private async Task ExitNodeButtonClick(string? peerId)
    {
        if (peerId is null)
        {
            return;
        }

        Peer? peer = Status?.Peers?[peerId];
        if (peer is null)
        {
            return;
        }

        if (peer.ExitNode.HasValue && !peer.ExitNode.Value)
        {
            List<IPAddress> ipAddresses = new();
            foreach (string item in peer?.TailscaleIPs?.ToArray() ?? Array.Empty<string>())
            {
                ipAddresses.Add(IPAddress.Parse(item));
            }

            await Connect(ipAddresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));
        }
        else
        {
            await Disconnect();
        }

        await GetStatus();
    }
}
