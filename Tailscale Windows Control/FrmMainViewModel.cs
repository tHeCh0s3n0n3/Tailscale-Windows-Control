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
    private static readonly Icon ConnectedIconOverlay = new(AppContext.GetManifestResourceStream("Tailscale_Windows_Control.Icons.Tailscale_connected_arrow.ico") ?? Stream.Null);

    public string TailscaleLocation { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskbarIconOverlayText))]
    public partial string StatusLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? CommandResult { get; set; }

    [ObservableProperty]
    public partial TailscaleStatus? Status { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskbarIconOverlay))]
    [NotifyPropertyChangedFor(nameof(TaskbarIconOverlayText))]
    public partial bool IsConnected { get; set; }

    public Icon? TaskbarIconOverlay
        => IsConnected ? ConnectedIconOverlay : null;

    public string TaskbarIconOverlayText
        => IsConnected ? StatusLabel : string.Empty;

    [ObservableProperty]
    public partial bool StateChanged { get; set; } = false;

    private readonly ProcessStartInfo _processStartInfoDefault;

    public FrmMainViewModel(string tailscaleExecutablePath)
    {
        TailscaleLocation = tailscaleExecutablePath;

        _processStartInfoDefault
            = new ProcessStartInfo(TailscaleLocation)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            LoadUserProfile = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };
    }

    /// <summary>
    /// Configures Tailscale to use a specific exit node.
    /// </summary>
    /// <param name="ipAddress">IP Address of the exit node to route all non-LAN traffic through.</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException" />
    /// <inheritdoc cref="ExecuteTailscaleCommand(string, CancellationToken)" path="/exception"/>
    private async Task Connect(IPAddress? ipAddress, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        CommandResult = await ExecuteTailscaleCommand($"up --reset --exit-node={ipAddress} --exit-node-allow-lan-access"
                                                      , cancellationToken);

        StatusLabel = "Connecting...";
    }

    /// <summary>
    /// Configures Tailscale to reset the use of an exit node.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <inheritdoc cref="ExecuteTailscaleCommand(string, CancellationToken)" path="/exception"/>
    private async Task Disconnect(CancellationToken cancellationToken = default)
    {
        CommandResult = await ExecuteTailscaleCommand("up --reset --exit-node=", cancellationToken);
        StatusLabel = "Disconnecting...";
    }

    /// <summary>
    /// Executes the Tailscale command as a separate process and captures the output.
    /// </summary>
    /// <param name="arguments">CLI arguments to pass on to tailscale.exe</param>
    /// <param name="cancellationToken"></param>
    /// <returns>All text emitted by Tailscale to standard output (trimmed).</returns>
    /// <inheritdoc cref="Process.Start()" path="/exception"/>
    /// <inheritdoc cref="StreamReader.ReadToEndAsync(CancellationToken)" path="/exception"/>
    /// <inheritdoc cref="Process.WaitForExitAsync(CancellationToken)" path="/exception"/>
    [RelayCommand]
    private async Task<string> ExecuteTailscaleCommand(string arguments, CancellationToken cancellationToken = default)
    {
        _processStartInfoDefault.Arguments = arguments;

        ProcessTextOutput capturedText
            = await Process.RunAndCaptureTextAsync(_processStartInfoDefault, cancellationToken);
        return capturedText.StandardOutput.Trim();
    }

    /// <summary>
    /// Get the current staus from Tailscale manually.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <inheritdoc cref="GetStatus(CancellationToken)" path="/exception"/>
    [RelayCommand]
    private async Task GetStatusManually(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            StatusLabel = "Updating Status...";
        }
        await GetStatus(cancellationToken);
    }

    /// <summary>
    /// Get the current status from Tailscale
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <inheritdoc cref="ExecuteTailscaleCommand(string, CancellationToken)" path="/exception"/>
    /// <inheritdoc cref="string.Replace(string, string?)" path="/exception"/>
    /// <inheritdoc cref="MemoryStream(byte[])" path="/exception"/>
    /// <inheritdoc cref="JsonSerializer.DeserializeAsync{TValue}(Stream, JsonSerializerOptions?, CancellationToken)" path="/exception"/>
    /// <inheritdoc cref="System.Linq.Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" path="/exception"/>
    /// <inheritdoc cref="System.Linq.Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" path="/exception"/>
    [RelayCommand]
    private async Task GetStatus(CancellationToken cancellationToken = default)
    {
        string output = await ExecuteTailscaleCommand("status --json", cancellationToken);
        output = output.Replace("\r\n", "\n")
                       .Replace("\n\r", "\n")
                       .Replace("\r", "\n")
                       .Replace("\n", Environment.NewLine);
        if (CommandResult is null
            || !CommandResult.Equals(output))
        {
            CommandResult = output;
        }

        using MemoryStream ms = new(Encoding.UTF8.GetBytes(output));
        Status = await JsonSerializer.DeserializeAsync<TailscaleStatus>(ms, options: null, cancellationToken: cancellationToken);
        
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

    /// <summary>
    /// Handles everything required when an exit node button is clicked.
    /// </summary>
    /// <param name="peerId">The peer ID of the exit node.</param>
    /// <param name="cancellationToken"></param>
    /// <inheritdoc cref="Nullable{T}.Value" path="/exception"/>
    /// <inheritdoc cref="IPAddress.Parse(string)" path="/exception"/>
    /// <inheritdoc cref="Connect(IPAddress?, CancellationToken)" path="/exception"/>
    /// <inheritdoc cref="System.Linq.Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" path="/exception"/>
    /// <inheritdoc cref="Disconnect(CancellationToken)" path="/exception"/>
    /// <inheritdoc cref="GetStatus(CancellationToken)" path="/exception"/>
    [RelayCommand]
    private async Task ExitNodeButtonClick(string? peerId, CancellationToken cancellationToken = default)
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
            List<IPAddress> ipAddresses = [];
            foreach (string item in peer.TailscaleIPs?.ToArray() ?? [])
            {
                ipAddresses.Add(IPAddress.Parse(item));
            }

            await Connect(ipAddresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                          , cancellationToken);
            StateChanged = true;
        }
        else
        {
            await Disconnect(cancellationToken);
            StateChanged = true;
        }

        await GetStatus(cancellationToken);
    }
}
