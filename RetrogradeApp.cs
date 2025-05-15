using Gtk;
using Microsoft.Extensions.Logging;
using System.IO.Ports;
using Dn500BD.Retrograde;

namespace Dn500BD;

public class RetrogradeApp(ILogger logger, Action<ISerialPortService> onSerialReady)
{
    private IDenonRemote? _remote;
    private ISerialPortService? _serial;
    private Box? _layout;
    private Button[] _controlButtons = [];
    private bool _isConnecting = false;

    private readonly ILogger _logger = logger;
    private readonly Action<ISerialPortService> _onSerialReady = onSerialReady;
    private readonly Box _timecodeBox = new(Orientation.Vertical, 5);
    private readonly ComboBoxText _portDropdown = [];
    private readonly ScrolledWindow _scrollWindow = [];

    public void InitializeUI(Box mainLayout)
    {
        _layout = mainLayout;

        // Port selection
        var portSelectionLayout = new Box(Orientation.Horizontal, 5);
        PopulateSerialPorts();
        portSelectionLayout.PackStart(_portDropdown, true, true, 0);
        mainLayout.PackStart(portSelectionLayout, false, false, 0);

        // Connect button
        var connectButton = new Button("Connect");
        connectButton.Clicked += OnConnectClick;
        mainLayout.PackStart(connectButton, false, false, 0);

        // Timecode scroll area
        _scrollWindow.Add(_timecodeBox);
        _scrollWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        mainLayout.PackStart(_scrollWindow, true, true, 0);
    }

    private void PopulateSerialPorts()
    {
        _portDropdown.RemoveAll();
        foreach (string port in SerialPort.GetPortNames())
            _portDropdown.AppendText(port);
        if (string.IsNullOrEmpty(_portDropdown.ActiveText))
            _portDropdown.Active = 0;
    }

    private void ToggleButtons(bool enabled)
    {
        foreach (var button in _controlButtons)
            button.Sensitive = enabled;
    }

    private async void OnButtonClick(Button button, DenonCommand command)
    {
        button.Sensitive = false;
        try
        {
            if (_remote != null && await _remote.SendCommandAsync(command))
                _logger.LogInformation("{Label} sent successfully.", command.Label);
            else
                _logger.LogWarning("Failed to send command: {Label}", command.Label);
        }
        finally
        {
            button.Sensitive = true;
        }
    }
    private async void OnConnectClick(object? sender, EventArgs args)
    {
        if (_isConnecting)
            return;

        _isConnecting = true;

        try
        {
            string selectedPort = _portDropdown.ActiveText ?? string.Empty;

            if (string.IsNullOrWhiteSpace(selectedPort))
            {
                _logger.LogWarning("No serial port selected.");
                return;
            }

            _serial?.Close();
            _serial = new SerialPortService();

            await Task.Run(() => _serial.Open(selectedPort));

            _onSerialReady(_serial);
            _logger.LogInformation("Connected to port: {Port}", selectedPort);

            _remote = new DenonRemoteController(_serial, _logger);

            // Safely update UI on the GTK main thread
            GLib.Idle.Add(() =>
            {
                BuildCommandButtons();
                ToggleButtons(true);
                return false; // Run once
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to port: {Port}", _portDropdown.ActiveText);
        }
        finally
        {
            _isConnecting = false;
        }
    }

    private static Box CreateButtonRow(params Widget?[] widgets)
    {
        var row = new Box(Orientation.Horizontal, 5);
        foreach (var widget in widgets)
            row.PackStart(widget ?? new Label(), true, true, 0);
        return row;
    }

    private void BuildCommandButtons()
    {
        if (_layout == null || _remote == null)
            return;

        var commands = _remote.GetAvailableCommands();
        _controlButtons = new Button[commands.Count()];

        int i = 0;
        foreach (var command in commands)
        {
            var button = new Button(command.Label);
            button.Clicked += (sender, args) => OnButtonClick(button, command);
            _controlButtons[i++] = button;
        }

        ToggleButtons(false);

        AddDirectionPadLayout();

        _layout.ShowAll(); // ✅ Needed to show new widgets
    }

    private void AddDirectionPadLayout()
    {
        if (_controlButtons.Length < 6)
        {
            _logger.LogError("Not enough commands to build direction pad layout.");
            return;
        }

        _layout!.PackStart(CreateButtonRow(null, _controlButtons[1], null), true, true, 0); // Up
        _layout!.PackStart(CreateButtonRow(_controlButtons[3], _controlButtons[5], _controlButtons[4]), true, true, 0); // Left, Enter, Right
        _layout!.PackStart(CreateButtonRow(null, _controlButtons[2], null), true, true, 0); // Down
        _layout!.PackStart(CreateButtonRow(_controlButtons[0]), true, true, 0); // Home
    }
}
