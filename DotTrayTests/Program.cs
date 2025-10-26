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

        var tempPath = CreateTestIcon() ?? throw new InvalidOperationException("Icon could not be created");

        var tray = await NotifyIcon.RunAsync(tempPath, cts.Token);

        var menuItem = tray.MenuItems.Add("Test");
        menuItem.BackgroundColor = new TrayColor(255, 0, 0);
        menuItem.BackgroundDisabledColor = new TrayColor(0, 255, 0);
        menuItem.BackgroundHoverColor = new TrayColor(0, 0, 255);
        menuItem.IsChecked = true;
        menuItem.Clicked = (args) =>
        {
            Console.WriteLine(args.MenuItem.IsChecked.HasValue ? args.MenuItem.IsChecked.Value : "NULL");
            args.MenuItem.Text = "Neuer Text";
        };

        tray.MenuItems.AddSeparator();

        menuItem = tray.MenuItems.Add("Test 2");
        menuItem.BackgroundColor = new TrayColor(255, 255, 255, 50);

        menuItem = menuItem.SubMenu.Add("Test Sub 1");

        menuItem.IsChecked = false;
        menuItem.IsDisabled = true;
        menuItem.Clicked = (args) => Console.WriteLine(args.MenuItem.IsChecked.HasValue ? args.MenuItem.IsChecked.Value : "NULL");

        var separator = tray.MenuItems.AddSeparator();
        separator.LineColor = new TrayColor(0, 255, 255);
        separator.LineThickness = 5f;

        menuItem = tray.MenuItems.Add("Exit");
        menuItem.TextColor = new TrayColor(255, 0, 0);
        menuItem.Clicked = _ => cts.Cancel();

        tray.SetToolTip("🔔 This is a long string with emoji 😊 and more");

        tray.MouseButtons = MouseButton.Left;
        tray.MenuShowing += args => Console.WriteLine("Showing: " + args);
        tray.MenuHiding += () => Console.WriteLine("Hiding");

        Console.ReadLine();

        Console.WriteLine("Showing Balloon Notification");
        tray.ShowBalloon(new BalloonNotification
        {
            Icon = BalloonNotificationIcon.User,
            Title = "Error - Something went wrong",
            Message = "You have done something wrong. You're cooked :(",
            NoSound = false
        });

        Console.ReadLine();

        Console.WriteLine("Adding Extra menu item");
        tray.MenuItems.Add("---------------- EXTRA ----------------");
        ((MenuItem)tray.MenuItems[0]).Text = ((MenuItem)tray.MenuItems[0]).Text + " - NEW";

        Console.ReadLine();

        Console.WriteLine("Destroying Tray Icon");
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