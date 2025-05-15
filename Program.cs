using Gtk;
using Microsoft.Extensions.Logging;
using Dn500BD.Retrograde;

namespace Dn500BD;
public static class Program
{
    public static void Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Information);
        });

        ILogger logger = loggerFactory.CreateLogger("DN500BD");
        ISerialPortService? serialToDispose = null;

        Application.Init();

        var retrograde = new RetrogradeApp(logger, serial => serialToDispose = serial);
        var window = new MainWindow();
        var layout = new Box(Orientation.Vertical, 5);

        retrograde.InitializeUI(layout);
        window.Add(layout);
        window.ShowAll();

        Application.Run();

        serialToDispose?.Dispose(); // Cleanup after loop
    }
}
