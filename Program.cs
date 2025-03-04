using Gdk;
using Gtk;
using System.IO.Ports;

class MyWindow : Gtk.Window
{
    public MyWindow() : base("DN 500BD Retrograde")
    {
        SetDefaultSize(800, 640); // Set window size to 800x640
    }

    protected override bool OnDeleteEvent(Event e)
    {
        Application.Quit();
        return true;
    }
}

class Retrograde
{
    private static SerialPort? _serialPort;
    private static ComboBoxText _portDropdown = new();
    private static VBox _timecodeBox = new();
    private static ScrolledWindow _scrollWindow = new();
    private static Button[] _controlButtons;

    private static readonly Dictionary<string, string> Commands = new()
    {
        { "Home", "@0PCHM\r" },
        { "Up", "@0PCCUSR3\r" },
        { "Down", "@0PCCUSR4\r" },
        { "Left", "@0PCCUSR1\r" },
        { "Right", "@0PCCUSR2\r" },
        { "Enter", "@0PCENTR\r" }
    };

    private static void InitializeUI(Box mainLayout)
    {
        // Port Selection UI
        var portSelectionLayout = new Box(Orientation.Horizontal, 5);
        PopulateSerialPorts();
        portSelectionLayout.PackStart(_portDropdown, true, true, 0);
        mainLayout.PackStart(portSelectionLayout, false, false, 0);

        // Connect Button
        var connectButton = new Button("Connect");
        connectButton.Clicked += OnConnectClick;
        mainLayout.PackStart(connectButton, false, false, 0);

        // Create Buttons Properly
        _controlButtons = new Button[Commands.Count];
        int i = 0;
        foreach (var (label, command) in Commands)
        {
            var button = new Button(label);
            button.Clicked += (sender, args) => OnButtonClick(button, command, label);
            _controlButtons[i++] = button; // Assign to array in correct order
        }
        ToggleButtons(false);

        // Arrange Buttons in UI
        mainLayout.PackStart(CreateButtonRow(null, _controlButtons[1], null), true, true, 0);
        mainLayout.PackStart(CreateButtonRow(_controlButtons[3], _controlButtons[5], _controlButtons[4]), true, true, 0);
        mainLayout.PackStart(CreateButtonRow(null, _controlButtons[2], null), true, true, 0);
        mainLayout.PackStart(CreateButtonRow(_controlButtons[0]), true, true, 0);

        // Timecode Entry Section
        _scrollWindow.AddWithViewport(_timecodeBox);
        _scrollWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        mainLayout.PackStart(_scrollWindow, true, true, 0);
    }

    private static void PopulateSerialPorts()
    {
        _portDropdown.RemoveAll();
        foreach (string port in SerialPort.GetPortNames())
            _portDropdown.AppendText(port);
        if (string.IsNullOrEmpty(_portDropdown.ActiveText))
            _portDropdown.Active = 0;
    }

    private static void ToggleButtons(bool enabled)
    {
        foreach (var button in _controlButtons)
            button.Sensitive = enabled;
    }

    private static async void OnButtonClick(Button button, string command, string message)
    {
        button.Sensitive = false;
        try
        {
            if (await SendDataAsync(command))
                Console.WriteLine(message);
            else
                Console.WriteLine($"Failed to send {message} command. Check device connection.");
        }
        finally
        {
            button.Sensitive = true;
        }
    }

    private static void OnConnectClick(object? sender, EventArgs args)
    {
        _serialPort?.Close();
        string selectedPort = _portDropdown.ActiveText ?? "";

        if (string.IsNullOrEmpty(selectedPort))
        {
            Console.WriteLine("No serial port selected.");
            return;
        }

        _serialPort = new SerialPort(selectedPort, 115200) { ReadTimeout = 400 };
        _serialPort.Open();
        ToggleButtons(true);
    }

    public static void Main()
    {
        Application.Init();
        var window = new MyWindow();
        var mainLayout = new Box(Orientation.Vertical, 5);
        InitializeUI(mainLayout);
        window.Add(mainLayout);
        window.ShowAll();
        Application.Run();
    }

    private static Box CreateButtonRow(params Widget?[] widgets)
    {
        var row = new Box(Orientation.Horizontal, 5);
        foreach (var widget in widgets)
            row.PackStart(widget ?? new Label(), true, true, 0);
        return row;
    }

    private static async Task<bool> SendDataAsync(string command)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
        {
            Console.WriteLine("Error: Serial port is not open.");
            return false;
        }

        int timeoutMs = 3000;
        using var cts = new CancellationTokenSource(timeoutMs);

        try
        {
            var commandBytes = System.Text.Encoding.ASCII.GetBytes(command);
            Task writeTask = _serialPort.BaseStream.WriteAsync(commandBytes.AsMemory(0, commandBytes.Length), cts.Token).AsTask();
            if (await Task.WhenAny(writeTask, Task.Delay(timeoutMs, cts.Token)) != writeTask)
                throw new TimeoutException("Writing to serial port timed out.");

            await _serialPort.BaseStream.FlushAsync(cts.Token);
            string response = await ReadDataWithTimeoutAsync(cts.Token);
            if (string.IsNullOrEmpty(response))
                throw new TimeoutException("Device did not respond within 3 seconds.");

            return true;
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Timeout Error: {ex.Message}");
            return false;
        }
    }

    private static async Task<string> ReadDataWithTimeoutAsync(CancellationToken token)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not open.");

        var buffer = new byte[1];
        string receivedData = "";

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            Task readTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        int bytesRead = await _serialPort.BaseStream.ReadAsync(buffer.AsMemory(0, 1), cts.Token);
                        if (bytesRead > 0)
                        {
                            char receivedChar = (char)buffer[0];
                            if (receivedChar == (char)255) break;
                            receivedData += receivedChar;
                        }
                    }
                    else await Task.Delay(100, cts.Token);
                }
            }, cts.Token);

            if (await Task.WhenAny(readTask, Task.Delay(3000, cts.Token)) != readTask)
                throw new TimeoutException("Device did not respond within 3 seconds.");
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Device did not respond within 3 seconds.");
        }

        return receivedData;
    }
}
