using Gdk;
using Gtk;
using System;
using System.IO.Ports;

class MyWindow : Gtk.Window
{
    public MyWindow() : base("DN 500BD Retrograde") { }

    protected override bool OnDeleteEvent(Event e)
    {
        Application.Quit();
        return true;
    }
}

class Retrograde
{
    private static SerialPort? _serialPort;
    private static Entry _portBox = new();
    private static VBox _timecodeBox = new();
    private static ScrolledWindow _scrollWindow = new();
    private static Button[] _controlButtons;

    private const string HomeCommand = "@0PCHM\r";
    private const string UpCommand = "@0PCCUSR3\r";
    private const string DownCommand = "@0PCCUSR4\r";
    private const string LeftCommand = "@0PCCUSR1\r";
    private const string RightCommand = "@0PCCUSR2\r";
    private const string EnterCommand = "@0PCENTR\r";
    private const string TimecodeCommand = "@0PCTMD";

    private static string ReadData()
    {
        string receivedData = "";
        byte tempByte = (byte)_serialPort!.ReadByte();

        while (tempByte != 255)
        {
            receivedData += (char)tempByte;
            tempByte = (byte)_serialPort.ReadByte();
        }

        return receivedData;
    }

    private static bool SendData(string command, string? parameter = null)
    {
        string fullCommand = parameter != null ? command + parameter + "\r" : command;
        _serialPort?.Write(fullCommand);
        return true;
    }

    private static void ToggleButtons(bool enabled)
    {
        foreach (var button in _controlButtons)
        {
            button.Sensitive = enabled;
        }
    }

    private static void OnButtonClick(string command, string message)
    {
        SendData(command);
        Console.WriteLine(message);
    }

    private static void OnConnectClick(object? sender, EventArgs args)
    {
        _serialPort?.Close();
        _serialPort = new SerialPort(_portBox.Text, 115200) { ReadTimeout = 400 };
        _serialPort.Open();
        ToggleButtons(true);
    }

    public static void Main()
    {
        Application.Init();

        var window = new MyWindow();
        var mainLayout = new Box(Orientation.Vertical, 5);

        // Port Entry
        var portEntryLayout = new Box(Orientation.Horizontal, 5);
        portEntryLayout.PackStart(_portBox, true, true, 0);
        mainLayout.PackStart(portEntryLayout, false, false, 0);

        // Connect Button
        var connectButton = new Button("Connect");
        connectButton.Clicked += OnConnectClick;
        var connectLayout = new Box(Orientation.Horizontal, 5);
        connectLayout.PackStart(connectButton, true, true, 0);
        mainLayout.PackStart(connectLayout, false, false, 0);

        // Navigation Buttons
        var upButton = new Button("^");
        upButton.Clicked += (sender, args) => OnButtonClick(UpCommand, "Up");

        var leftButton = new Button("<");
        leftButton.Clicked += (sender, args) => OnButtonClick(LeftCommand, "Left");
        var enterButton = new Button("Enter");
        enterButton.Clicked += (sender, args) => OnButtonClick(EnterCommand, "Enter");
        var rightButton = new Button(">");
        rightButton.Clicked += (sender, args) => OnButtonClick(RightCommand, "Right");
        var downButton = new Button("V");
        downButton.Clicked += (sender, args) => OnButtonClick(DownCommand, "Down");
        var homeButton = new Button("Home");
        homeButton.Clicked += (sender, args) => OnButtonClick(HomeCommand, "Home");

        _controlButtons = new Button[] { upButton, leftButton, enterButton, rightButton, downButton, homeButton };
        ToggleButtons(false);

        mainLayout.PackStart(CreateButtonRow(null, upButton, null), true, true, 0);
        mainLayout.PackStart(CreateButtonRow(leftButton, enterButton, rightButton), true, true, 0);
        mainLayout.PackStart(CreateButtonRow(null, downButton, null), true, true, 0);
        mainLayout.PackStart(CreateButtonRow(homeButton), true, true, 0);

        // Timecode Entry Section
        _scrollWindow.AddWithViewport(_timecodeBox);
        _scrollWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        mainLayout.PackStart(_scrollWindow, true, true, 0);

        window.Add(mainLayout);
        window.ShowAll();
        Application.Run();
    }

    private static Box CreateButtonRow(params Widget?[] widgets)
    {
        var row = new Box(Orientation.Horizontal, 5);
        foreach (var widget in widgets)
        {
            row.PackStart(widget ?? new Label(), true, true, 0);
        }
        return row;
    }
}
