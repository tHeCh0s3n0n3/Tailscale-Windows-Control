using Microsoft.Win32;
using System.ComponentModel;
using System.Reflection;
using Timer = System.Windows.Forms.Timer;

namespace Tailscale_Windows_Control;

public partial class FrmMain : Form
{
    public readonly FrmMainViewModel _vm;
    public readonly Button _btnGetStatus;

    /// <summary>
    /// Global timer which when ticked polls TailScale for status changes.
    /// </summary>
    public readonly Timer _timer;
    private List<Peer>? _exitNodes;

    private readonly NotifyIcon taskbarIcon = new();

    /// <summary>
    /// List of possible TailScale executable locations.
    /// </summary>
    private readonly List<string> _tailScaleLocations =
    [
        @"C:\Program Files (x86)\Tailscale IPN\tailscale.exe"
        , @"C:\Program Files (x86)\Tailscale\tailscale.exe"
        , @"C:\Program Files\Tailscale IPN\tailscale.exe"
        , @"C:\Program Files\Tailscale\tailscale.exe"
    ];

    /// <summary>
    /// Flag to indicate that we are still in start-up mode.
    /// </summary>
    private bool m_isStartup = true;

    /// <summary>
    /// Time in milliseconds to tick the global timer at startup which polls TailScale.
    /// </summary>
    private const int m_startupTimerInterval = 100;

    /// <summary>
    /// Time in milliseconds to tick the global timer which polls TailScale.
    /// </summary>
    private const int m_regularTimerInterval = 2 * 1000;

    /// <inheritdoc cref="SystemEvents.UserPreferenceChanged" path="/exception"/>
    /// <inheritdoc cref="SetupViewModel" path="/exception"/>
    /// <inheritdoc cref="MessageBox.Show(string?, string?, MessageBoxButtons, MessageBoxIcon)" path="/exception"/>
    /// <inheritdoc cref="Environment.Exit(int)" path="/exception"/>
    /// <inheritdoc cref="ControlBindingsCollection.Add(string, object, string?)" path="/exception"/>
    /// <inheritdoc cref="Assembly.GetManifestResourceStream(string)" path="/exception"/>
    public FrmMain()
    {
        InitializeComponent();

        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

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
            AutoSize = true
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
        Icon = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("Tailscale_Windows_Control.Icons.Tailscale.ico") ?? Stream.Null);
    }

    /// <summary>
    /// Event handler for when a UserPreference changes. This is used to reset the SystemTheme (if required).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.Color)
        {
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(ApplySystemTheme);
            }
        }
    }

    /// <summary>
    /// Applies the system theme and ensures that the form is redrawn
    /// </summary>
    private void ApplySystemTheme()
    {
        if (IsDisposed) return;

        // Re-evaluating SystemColorMode.System queries the updated OS dark/light state
        Application.SetColorMode(SystemColorMode.System);

        // Force all open forms and child controls to redraw with the new SystemColors/VisualStyles
        foreach (Form form in Application.OpenForms)
        {
            form.Invalidate(true);
            form.Update();
        }
    }

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    /// <inheritdoc cref="SystemEvents.UserPreferenceChanged" path="/exception"/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Mandatory: SystemEvents is static and will leak the Form if not unsubscribed
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Initializes the ViewModel
    /// </summary>
    /// <returns>A fully initialized <see cref="FrmMainViewModel"/> or <see langword="null"/> on failure to initialize</returns>
    /// <inheritdoc cref="System.Linq.Enumerable.Where" path="/exception"/>
    /// <inheritdoc cref="System.Linq.Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})" path="/exception"/>
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

    /// <summary>
    /// Checks if Tailscale is intalled
    /// </summary>
    /// <returns><see langword="true" /> if Tailscale is detected or <see langword="false"/> otherwise.</returns>
    /// <inheritdoc cref="RegistryKey.OpenSubKey(string)" path="/exception"/>
    /// <inheritdoc cref="RegistryKey.GetSubKeyNames" path="/exception"/>
    /// <inheritdoc cref="RegistryKey.GetValue(string?)" path="/exception"/>
    /// <inheritdoc cref="System.Linq.Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" path="/exception"/>
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

    /// <summary>
    /// Event handler for timer tick
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <inheritdoc cref="CommunityToolkit.Mvvm.Input.IAsyncRelayCommand.ExecuteAsync(object?)" path="/exception"/>
    /// <inheritdoc cref="SetConnectionButtons" path="/exception"/>
    /// <inheritdoc cref="TaskbarService.SetOverlay(nint, Icon?, string)" path="/exception"/>
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

        if (_vm.StateChanged)
        {
            TaskbarService.SetOverlay(this.Handle, _vm.TaskbarIconOverlay, _vm.TaskbarIconOverlayText);
            _vm.StateChanged = false;
        }
    }

    /// <summary>
    /// Event handler triggered when the GetStatus button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentNullException" />
    /// <inheritdoc cref="CommunityToolkit.Mvvm.Input.IAsyncRelayCommand.ExecuteAsync(object?)" path="/exception"/>    /// 
    /// <inheritdoc cref="SetConnectionButtons" path="/exception"/>
    private async void BtnGetStatus_Click(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        await _vm.GetStatusManuallyCommand.ExecuteAsync(null);
        SetConnectionButtons();
    }

    /// <summary>
    /// Creates and configures the exit node connection buttons.
    /// </summary>
    /// <inheritdoc cref="System.Linq.Enumerable.ToArray{TSource}(IEnumerable{TSource})" path="/exception"/>
    /// <inheritdoc cref="System.Linq.Enumerable.ExceptBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TKey}, Func{TSource, TKey})" path="/exception"/>
    /// <inheritdoc cref="System.Linq.Enumerable.Any{TSource}(IEnumerable{TSource})" path="/exception"/>
    /// <inheritdoc cref="Nullable{T}.Value" path="/exception"/>
    /// <inheritdoc cref="BtnExitNode_Click(object?, EventArgs)" path="/exception"/>
    /// <inheritdoc cref="Control.ControlCollection.Add(Control)" path="/exception"/>
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
                AutoSize = true
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
    }

    /// <summary>
    /// Event handler triggered when an exit node connection button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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