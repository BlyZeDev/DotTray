namespace DotTrayTests;

using DotTray;
using DotTray.Drawing;
using DotTray.Drawing.Items;

public static class DrawingIcon
{
    public static async Task<IDisposable> RunAsync(string iconPath, CancellationTokenSource cts)
    {
        var icon = await NotifyIcon.RunDrawingAsync(iconPath, cts.Token);
        var handler = icon.Handler;

        handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Standard Action";
            x.Interacted = args =>
            {
                if (args.Type is not ItemInteractionType.MouseLeftUp) return;

                Console.WriteLine("Action executed!");
            };
        });

        handler.MenuItems.Add<CheckItem>(x =>
        {
            x.Text = "Enable Background Sync";
            x.IsChecked = true;
            x.Interacted = args =>
            {
                if (args.Type is not ItemInteractionType.MouseLeftUp) return;

                args.KeepMenuOpen = true;
                Console.WriteLine($"Sync is now {(x.IsChecked ? "ON" : "OFF")}");
            };
        });

        handler.MenuItems.Add<SeparatorItem>();

        handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Advanced Settings";

            x.Items.Add<MenuItem>(x =>
            {
                x.Text = "Dynamic Item (Click to update time)";
                x.Interacted = args =>
                {
                    if (args.Type is not ItemInteractionType.MouseLeftUp) return;

                    x.Text = $"Last clicked: {DateTime.Now:HH:mm:ss}";
                };
            });

            x.Items.Add<MenuItem>(x =>
            {
                x.Text = "Premium Feature (Locked)";
                x.IsDisabled = true;
            });
        });

        handler.MenuItems.Add<SeparatorItem>();

        handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Exit Application";
            x.Interacted = args =>
            {
                if (args.Type is not ItemInteractionType.MouseLeftUp) return;

                Console.WriteLine("Exiting gracefully via tray menu...");
                cts.Cancel();
            };
        });

        Console.WriteLine("Drawing icon is running");

        return icon;
    }
}