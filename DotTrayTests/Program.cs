namespace DotTrayTests;

using DotTray;
using System.Runtime.Versioning;

sealed class Program
{
    [SupportedOSPlatform("windows")]
    static async Task Main()
    {
        var cts = new CancellationTokenSource();
        IEnumerable<IMenuItem> menuItems =
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

        var tray = await NotifyIcon.RunAsync(nint.Zero, cts.Token);
        tray.SetToolTip("Test ToolTip");
        tray.SetMenuItems(menuItems);

        Console.WriteLine("End is reached!");

        Console.ReadLine();

        cts.Cancel();
        tray.Dispose();

        Console.ReadLine();
    }
}