namespace DotTrayTests;

using DotTray;
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
                Click = (sender, args) =>
                {
                    Console.WriteLine("Test");
                    sender.Text = "Neuer Text";
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
                Click = (_, _) => cts.Cancel()
            }
        ];

        var tray = await NotifyIcon.RunAsync(@"C:\Users\leons\OneDrive\Desktop\!Programmierung\NuGetPackages\DotTray\DotTray\icon.ico", menuItems, cts.Token);
        var tray2 = await NotifyIcon.RunAsync(@"C:\Users\leons\OneDrive\Desktop\!Programmierung\NuGetPackages\DotTray\DotTray\icon.ico", menuItems.Copy(), cts.Token);
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
    }
}