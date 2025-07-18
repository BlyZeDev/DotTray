namespace DotTrayTests;

using DotTray;

sealed class Program
{
    static async Task Main()
    {
        await Task.Yield();

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

        var tray = NotifyIcon.Run(nint.Zero, cts.Token);
        tray.ToolTip = "Test ToolTip";
        tray.MenuItems = menuItems;

        Console.WriteLine("End is reached!");

        Console.ReadLine();

        cts.Cancel();
        tray.Dispose();

        Console.ReadLine();
    }
}