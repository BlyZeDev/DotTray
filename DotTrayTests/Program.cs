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
        using (var icon = await NotifyIcon.RunAsync(@"C:\Users\leons\OneDrive\Desktop\!Programmierung\NuGetPackages\DotTray\icon.ico", cts.Token))
        {
            /*
            var menuItems = icon.MenuItems;

            var current = menuItems.AddItem("Item Number 1");
            current.Clicked = (args) => Console.WriteLine($"Clicked: Icon={args.Icon}, Item={args.MenuItem}, Mouse={args.MouseButton}");

            menuItems.AddItem("Item Number 2").IsDisabled = true;

            var separator = menuItems.AddSeparator();
            separator.LineColor = new TrayColor(0, 255, 255);
            separator.LineThickness = 5f;
            separator.BackgroundColor = new TrayColor(255, 0, 0);

            current = menuItems.AddItem("Item Number 3");
            current.SubMenu.AddItem("Submenu Item 1");
            */
            icon.SetToolTip("This is a NotifyIcon");
            icon.MouseButtons = MouseButton.Left | MouseButton.Right;
            icon.PopupShowing += args => Console.WriteLine($"Showing: {args}");
            icon.PopupHiding += () => Console.WriteLine("Hiding");

            icon.ShowBalloon(new BalloonNotification
            {
                Icon = BalloonNotificationIcon.User,
                Title = "Bold Title here",
                Message = "Message text here",
                NoSound = false
            });
            
            await Task.Delay(Timeout.Infinite, cts.Token);
        }

        /*
        var cts = new CancellationTokenSource();

        var tempPath = CreateTestIcon(StockIconId.Error) ?? throw new InvalidOperationException("Icon could not be created");
        var tempPath2 = CreateTestIcon(StockIconId.AudioFiles) ?? throw new InvalidOperationException("Icon could not be created");

        var tray = await NotifyIcon.RunAsync(tempPath, cts.Token);
        var defaultTray = await NotifyIcon.RunAsync(tempPath2, cts.Token);

        defaultTray.MenuItems.AddItem("Item No. 1");
        defaultTray.MenuItems.AddItem("Item No. 2");
        defaultTray.MenuItems.AddSeparator();
        defaultTray.MenuItems.AddItem("Item No. 3").IsChecked = false;
        defaultTray.MenuItems.AddSeparator();
        var testItem = defaultTray.MenuItems.AddItem("SubMenu here").SubMenu.AddItem("Submenu Test").SubMenu.AddItem("Submenu 2 Test").SubMenu.AddItem("Submenu 3 Test");
        testItem.SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").SubMenu.AddItem("---").Clicked += (args) => Console.WriteLine("tf you're doing here mate");
        defaultTray.MenuItems.AddSeparator();
        defaultTray.MenuItems.AddItem("Last Item");

        var menuItem = tray.MenuItems.AddItem("Test");
        menuItem.BackgroundColor = new TrayColor(255, 0, 0);
        menuItem.BackgroundDisabledColor = new TrayColor(0, 255, 0);
        menuItem.BackgroundHoverColor = new TrayColor(0, 0, 255);
        menuItem.IsChecked = true;
        menuItem.Clicked = (args) =>
        {
            Console.WriteLine(args.MenuItem.IsChecked.HasValue ? args.MenuItem.IsChecked.Value : "NULL");
            Console.WriteLine(args.MouseButton);
            args.MenuItem.Text = "Neuer Text";
        };

        tray.MenuItems.AddSeparator();

        menuItem = tray.MenuItems.AddItem("Test 2");
        menuItem.BackgroundColor = new TrayColor(255, 255, 255, 50);
        menuItem.Clicked = (args) =>
        {
            if (args.Icon.IsVisible) args.Icon.Hide();
            else args.Icon.Show();

            Console.WriteLine("Now visible: " + args.Icon.IsVisible);
        };

        menuItem = menuItem.SubMenu.AddItem("Test Sub 1");

        menuItem.IsChecked = false;
        menuItem.IsDisabled = true;
        menuItem.Clicked = (args) => Console.WriteLine(args.MenuItem.IsChecked.HasValue ? args.MenuItem.IsChecked.Value : "NULL");

        var separator = tray.MenuItems.AddSeparator();
        separator.LineColor = new TrayColor(0, 255, 255);
        separator.LineThickness = 5f;
        separator.BackgroundColor = new TrayColor(255, 0, 0);

        menuItem = tray.MenuItems.AddItem("Exit");
        menuItem.TextColor = new TrayColor(255, 0, 0);
        menuItem.Clicked = _ => cts.Cancel();

        tray.SetToolTip("🔔 This is a long string with emoji 😊 and more. Super duper insanely giga mega ultra long :o\nHow about getting a job Mr. Squidward 🤣😇😎🥰💩👹, This will not be seen :D");
        defaultTray.SetToolTip("What's up ma drilla");

        tray.MouseButtons = MouseButton.Left;
        tray.PopupShowing += args => Console.WriteLine("Showing: " + args);
        tray.PopupHiding += () => Console.WriteLine("Hiding");

        Console.ReadLine();

        tray.Show();
        Console.WriteLine("The icon is back");

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

        Console.WriteLine("Adding Extra menu item in 5 sec");
        tray.MenuItems.AddItem("---------------- EXTRA ----------------");
        ((MenuItem)tray.MenuItems[0]).Text = ((MenuItem)tray.MenuItems[0]).Text + " - NEW";

        await Task.Delay(5000);

        testItem.Text = "NEW TEXT AFTER 5 SECONDS !!!";

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

        */
    }

    [SupportedOSPlatform("windows")]
    private static string? CreateTestIcon(StockIconId id, StockIconOptions options = StockIconOptions.SmallIcon)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.CreateVersion7()}.ico");

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