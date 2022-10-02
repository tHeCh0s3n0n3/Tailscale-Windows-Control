using Timer = System.Windows.Forms.Timer;

namespace Tailscale_Windows_Control;

public partial class FrmMain : Form
{
    public readonly FrmMainViewModel _vm;
    public readonly Button _btnGetStatus;
    public readonly Timer _timer;
    private List<Peer>? _exitNodes;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FrmMain()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        InitializeComponent();
        
        if (File.Exists("C:\\Program Files (x86)\\Tailscale IPN\\tailscale.exe"))
        {
            _vm = new("C:\\Program Files (x86)\\Tailscale IPN\\tailscale.exe");
        }
        if (File.Exists("C:\\Program Files\\Tailscale IPN\\tailscale.exe"))
        {
            _vm = new("C:\\Program Files\\Tailscale IPN\\tailscale.exe");
        }
        if (_vm is null)
        {
            Application.Exit();
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
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        await _vm.GetStatusCommand.ExecuteAsync(null);
        SetConnectionButtons();
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
    }

    private async void BtnExitNode_Click(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        string? peerId = ((Button)sender).Tag.ToString();

        if (peerId is null)
        {
            return;
        }

        await _vm.ExitNodeButtonClickCommand.ExecuteAsync(peerId);
    }
}