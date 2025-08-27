namespace DotTrayTests;

using DotTray;
using System.Drawing;
using System.Runtime.Versioning;

sealed class Program
{
    [SupportedOSPlatform("windows")]
    static async Task Main()
    {
        var cts = new CancellationTokenSource();
        MenuItemCollection menuItems =
        [
            new MenuItem
            {
                Text = "Test",
                IsChecked = true,
                Click = (args) =>
                {
                    Console.WriteLine("Test");
                    args.MenuItem.Text = "Neuer Text";
                }
            },
            SeparatorItem.Instance,
            new MenuItem
            {
                Text = "Test 2",
                IsChecked = null,
                SubMenu =
                [
                    new MenuItem
                    {
                        Text = "Test Sub 1",
                        IsChecked = false,
                        IsDisabled = true
                    }
                ]
            },
            SeparatorItem.Instance,
            SeparatorItem.Instance,
            new MenuItem
            {
                Text = "EXIT",
                Click = _ => cts.Cancel()
            }
        ];

        var tempPath = CreateTestIcon() ?? throw new InvalidOperationException("Icon could not be created");

        var tray = await NotifyIcon.RunAsync(tempPath, menuItems, cts.Token);
        var tray2 = await NotifyIcon.RunAsync(tempPath, menuItems.Copy(), cts.Token);
        tray.SetToolTip("🔔 This is a long string with emoji 😊 and more");
        tray2.SetToolTip("Second Icon :)");

        Console.ReadLine();

        tray.MenuItems.Insert(tray.MenuItems.Count - 1, new MenuItem { Text = "New Item test" });
        tray.ShowBalloon(new BalloonNotification
        {
            Icon = BalloonNotificationIcon.User,
            Title = "Error - Something went wrong",
            Message = "You have done something wrong. You're cooked :(",
            NoSound = false
        });

        Console.ReadLine();

        cts.Cancel();
        tray.Dispose();

        Console.ReadLine();

        try
        {
            File.Delete(tempPath ?? "");
        }
        catch (Exception) { }
    }

    [SupportedOSPlatform("windows")]
    private static string? CreateTestIcon()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.CreateVersion7()}.ico");

        using (var icon = SystemIcons.GetStockIcon(StockIconId.Error, StockIconOptions.SmallIcon))
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