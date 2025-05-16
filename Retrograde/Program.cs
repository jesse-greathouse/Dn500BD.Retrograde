using Dn500BD.Retrograde.Infra;
using Dn500BD.Retrograde.UI;
using Gtk;
using Microsoft.Extensions.Logging;

namespace Dn500BD.Retrograde;
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
        var window = new MainWindow(() => serialToDispose?.Dispose());
        var layout = new Box(Orientation.Vertical, 5);

        retrograde.InitializeUI(layout);
        window.Add(layout);
        window.ShowAll();

        Application.Run();

        serialToDispose?.Dispose(); // Cleanup after loop
    }
}
