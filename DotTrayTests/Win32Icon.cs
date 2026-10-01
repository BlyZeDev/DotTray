namespace DotTrayTests;

using DotTray;
using DotTray.Default;

public static class Win32Icon
{
    public static async Task<IDisposable> RunAsync(string iconPath, CancellationTokenSource cts)
    {
        var icon = await NotifyIcon.RunAsync(iconPath, new Win32PopupMenuHandler(), cts.Token);
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

        Console.WriteLine("Win32 icon is running");

        return icon;
    }
}