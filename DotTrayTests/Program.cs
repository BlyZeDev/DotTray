[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
namespace DotTrayTests;

using DotTray;
using DotTray.Default;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

sealed class Program
{
    static async Task Main()
    {
        var cts = new CancellationTokenSource();

        using var icon = await NotifyIcon.RunAsync(
            CreateTestIcon(StockIconId.DeviceCamera),
            new DefaultPopupMenuHandler(),
            cts.Token);

        var handler = icon.Handler;

        handler.Items.Add(new MenuItem
        {
            Text = "Standard Action",
            Clicked = item => Console.WriteLine("Action executed!")
        });

        handler.Items.Add(new CheckItem
        {
            Text = "Enable Background Sync",
            IsChecked = true,
            Clicked = item => Console.WriteLine($"Sync is now {(item.IsChecked ? "ON" : "OFF")}")
        });

        handler.Items.Add(SeparatorItem.Instance);

        var submenu = new SubmenuItem { Text = "Advanced Settings" };

        submenu.Items.Add(new MenuItem
        {
            Text = "Dynamic Item (Click to update time)",
            Clicked = item => item.Text = $"Last clicked: {DateTime.Now:HH:mm:ss}"
        });

        submenu.Items.Add(new MenuItem
        {
            Text = "Premium Feature (Locked)",
            IsDisabled = true
        });

        handler.Items.Add(submenu);

        handler.Items.Add(SeparatorItem.Instance);

        handler.Items.Add(new MenuItem
        {
            Text = "Exit Application",
            Clicked = _ =>
            {
                Console.WriteLine("Exiting gracefully via tray menu...");
                cts.Cancel();
            }
        });

        Console.WriteLine("Tray icon is running. Right-click the system tray icon to see the menu.");
        Console.WriteLine("Press Enter to exit forcefully.");

        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    private static string? CreateTestIcon(StockIconId id, StockIconOptions options = StockIconOptions.ShellIconSize)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"temptesticon.ico");

        using (var icon = SystemIcons.GetStockIcon(id, options))
        {
            if (icon is null) return null;

            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                icon.Save(fileStream);
                fileStream.Flush();
            }
        }

        return tempPath;
    }
}