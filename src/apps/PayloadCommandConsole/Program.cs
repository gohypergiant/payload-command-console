using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using Serilog;

namespace PayloadCommandConsole
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static async Task Main(string[] args)
        {
            SerilogLogger.Initialize(new LoggerConfiguration()
                //.Filter.ByIncludingOnly(Matching.WithProperty("Area", LogArea.Layout))
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "{Area}: {Message}")
                .CreateLogger());
            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                var msg = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                    "Error",
                    ex.Message);

                await msg.Show();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();
    }
}
