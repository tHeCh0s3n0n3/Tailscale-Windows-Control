using Microsoft.WindowsAPICodePack.Taskbar;
using Timer = System.Windows.Forms.Timer;

namespace Tailscale_Windows_Control;

public partial class FrmMain : Form
{
    public bool InitSuccess { get; init; } = true;

    public readonly FrmMainViewModel _vm;
    public readonly Button _btnGetStatus;
    public readonly Timer _timer;
    private List<Peer>? _exitNodes;

    private readonly NotifyIcon taskbarIcon = new();

    private readonly List<string> _tailScaleLocations = new()
    {
        @"C:\Program Files (x86)\Tailscale IPN\tailscale.exe"
        , @"C:\Program Files (x86)\Tailscale\tailscale.exe"
        , @"C:\Program Files\Tailscale IPN\tailscale.exe"
        , @"C:\Program Files\Tailscale\tailscale.exe"
    };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FrmMain()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        InitializeComponent();
        
        foreach(string location in _tailScaleLocations)
        {
            if (File.Exists(location))
            {
                _vm = new(location);
            }
        }

        if (_vm is null)
        {
            MessageBox.Show("Cannot find TailScale, exiting.");
            InitSuccess = false;
            return;
        }

        _btnGetStatus = new()
        {
            FlatStyle = FlatStyle.Flat
            , AutoSize = true
            , Text = "Get Status"
        };
        _btnGetStatus.Click += BtnGetStatus_Click;

        _timer = new()
        {
            Interval = (int)(new TimeSpan(0, 0, 2)).TotalMilliseconds
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        lblStatusText.DataBindings.Add("Text", _vm, nameof(_vm.StatusLabel));
        this.DataBindings.Add("Icon", _vm, nameof(_vm.TaskbarIcon));
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        await _vm.GetStatusCommand.ExecuteAsync(null);
        SetConnectionButtons();

        if (_vm.TaskbarIconOverlay is not null)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            TaskbarManager.Instance.SetOverlayIcon(_vm.TaskbarIconOverlay, _vm.TaskbarIconOverlayText);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        else
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TaskbarManager.Instance.SetOverlayIcon(null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
        

    }

    private async void BtnGetStatus_Click(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        await _vm.GetStatusCommand.ExecuteAsync(null);
        SetConnectionButtons();
    }

    private void SetConnectionButtons()
    {
        List<Peer> newExitNodes = new();
        List<Button> exitNodeButtons = new();

        foreach (var peerItem in _vm.Status?.Peers?.ToArray() ?? Array.Empty<KeyValuePair<string, Peer>>())
        {
            Peer peer = peerItem.Value;
            if (peer is null)
            {
                continue;
            }
            if (!peer.ExitNodeOption.HasValue
                || !peer.ExitNodeOption.Value)
            {
                continue;
            }

            Button newButton = new()
            {
                FlatStyle = FlatStyle.Flat
                , AutoSize = true
                , Text = $"{(peer.ExitNode.HasValue && peer.ExitNode.Value ? "Disconnect" : "Connect")} {peer.HostName}"
                , Tag = peerItem.Key
            };
            newButton.Click += BtnExitNode_Click;

            newExitNodes.Add(peer);
            exitNodeButtons.Add(newButton);
        }

        if (_exitNodes is null
            || _exitNodes.Except(newExitNodes, new PeerComparer()).Any())
        {
            flpExitNodeButtons.Controls.Clear();
            flpExitNodeButtons.Controls.Add(_btnGetStatus);
            flpExitNodeButtons.Controls.AddRange(exitNodeButtons.ToArray());
            _exitNodes = newExitNodes;
        }

        taskbarIcon.Visible = true;
        taskbarIcon.Icon = _vm.TaskbarIcon;
    }

    private async void BtnExitNode_Click(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        string? peerId = ((Button)sender).Tag?.ToString();

        if (peerId is null)
        {
            return;
        }

        await _vm.ExitNodeButtonClickCommand.ExecuteAsync(peerId);
    }
}