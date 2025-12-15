using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace Tailscale_Windows_Control;

public partial class FrmMain : Form
{
    public readonly FrmMainViewModel _vm;
    public readonly Button _btnGetStatus;
    public readonly Timer _timer;
    private List<Peer>? _exitNodes;

    private readonly NotifyIcon taskbarIcon = new();

    private readonly List<string> _tailScaleLocations =
    [
        @"C:\Program Files (x86)\Tailscale IPN\tailscale.exe"
        , @"C:\Program Files (x86)\Tailscale\tailscale.exe"
        , @"C:\Program Files\Tailscale IPN\tailscale.exe"
        , @"C:\Program Files\Tailscale\tailscale.exe"
    ];

    private bool m_isStartup = true;
    private const int m_startupTimerInterval = 100;
    private const int m_regularTimerInterval = 2 * 1000;

    public FrmMain()
    {
        InitializeComponent();
        
        FrmMainViewModel? newVM = SetupViewModel();

        if (newVM is null)
        {
            MessageBox.Show("Cannot find Tailscale, exiting.", "Tailscale not found", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            Application.Exit(new CancelEventArgs(true));
            Environment.Exit(1);
        }
        else
        {
            _vm = newVM;
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
            Interval = m_startupTimerInterval
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        lblStatusText.DataBindings.Add("Text", _vm, nameof(_vm.StatusLabel));
        this.DataBindings.Add("Icon", _vm, nameof(_vm.TaskbarIcon));
    }

    private FrmMainViewModel? SetupViewModel()
    {
        if (Properties.Settings.Default.TailscaleLocation is not null
            && File.Exists(Properties.Settings.Default.TailscaleLocation))
        {
            return new(Properties.Settings.Default.TailscaleLocation);
        }

            string? foundLocation = _tailScaleLocations.Where(fl => File.Exists(fl))
                                                       .FirstOrDefault();
            if (foundLocation is not null)
            {
            return new(foundLocation);
            }

        if (!TailscaleIsInstalled())
        {
            return null;
        }

            using OpenFileDialog ofd = new()
            {
                Title = "Select Tailscale executable",
                Filter = "Tailscale Executable (tailscale.exe)|tailscale.exe|All Executables (*.exe)|*.exe|All Files (*.*)|*.*"
            };

            if (DialogResult.OK == ofd.ShowDialog()
                && File.Exists(ofd.FileName))
            {
                Properties.Settings.Default.TailscaleLocation = ofd.FileName;
                Properties.Settings.Default.Save();
            return new(Properties.Settings.Default.TailscaleLocation);
            }

        return null;
        }

    private static bool TailscaleIsInstalled()
    {
        RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
        if (key is null)
        {
            return false;
        }

        foreach(RegistryKey? subKey in key.GetSubKeyNames().Select(skn => key.OpenSubKey(skn)))
        {
            if (subKey is null)
            {
                continue;
            }

            if (subKey.GetValue("DisplayName") is not string displayName)
            {
                continue;
            }

            if (displayName.Contains("tailscale", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        await _vm.GetStatusCommand.ExecuteAsync(null);


        if (m_isStartup && (_vm.Status?.Peers?.Count ?? 0) > 0)
        {
            Timer? tmr = sender as Timer;
            tmr?.Interval = m_regularTimerInterval;
            m_isStartup = false;
        }

        SetConnectionButtons();

        if (_vm.TaskbarIconOverlay is not null)
        {
            TaskbarManager.Instance.SetOverlayIcon(_vm.TaskbarIconOverlay, _vm.TaskbarIconOverlayText!);
        }
        else
        {
            TaskbarManager.Instance.SetOverlayIcon(null, string.Empty);
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
        List<Peer> newExitNodes = [];
        List<Button> exitNodeButtons = [];

        foreach (KeyValuePair<string, Peer> peerItem in _vm.Status?.Peers?.ToArray() ?? [])
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
            flpExitNodeButtons.Controls.AddRange([.. exitNodeButtons]);
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