namespace DotTrayTests;

using AsyncAwaitBestPractices;
using DotTray;
using System.Drawing;
using System.Runtime.Versioning;
using System.Threading.Tasks;

sealed class Program
{
    [SupportedOSPlatform("windows")]
    static async Task Main()
    {
        var cts = new CancellationTokenSource();

        var tempPath = CreateTestIcon(StockIconId.Error) ?? throw new InvalidOperationException("Icon could not be created");
        var tempPath2 = CreateTestIcon((StockIconId)Random.Shared.Next(0, 141)) ?? throw new InvalidOperationException("Icon could not be created");
        
        var icon = await NotifyIcon.RunAsync(tempPath, cts.Token);
        var icon2 = NotifyIcon.Run(tempPath2, cts.Token);
        icon.SetToolTip("TEST");
        icon2.SetToolTip("TEST2");

        PeriodicAction(() =>
        {
            tempPath = CreateTestIcon((StockIconId)Random.Shared.Next(0, 141)) ?? throw new InvalidOperationException("Icon could not be created");

            icon.SetToolTip(Random.Shared.Next(0, 2) == 0 ? tempPath : null);
        }, TimeSpan.FromSeconds(6)).SafeFireAndForget();

        PeriodicAction(() =>
        {
            icon2.ShowBalloon(new BalloonNotification
            {
                Icon = BalloonNotificationIcon.User,
                Message = tempPath2,
                Title = "New Icon",
                NoSound = false
            });
        }, TimeSpan.FromSeconds(12)).SafeFireAndForget();
        /*
        icon.MenuItems.Add(x =>
        {
            x.Text = "Sync now";
        });
        icon.MenuItems.Add(x =>
        {
            x.Text = $"Next Sync in";
            x.IsDisabled = true;
        });
        icon.MenuItems.Add(x =>
        {
            x.Text = "Settings";
            x.SubMenu.Add(x =>
            {
                x.Text = "Open Application Folder";
            });
            x.SubMenu.Add(x =>
            {
                x.Text = "Autostart";
            });
            x.SubMenu.Add(x =>
            {
                x.Text = "Help";
            });
        });
        icon.MenuItems.Add();
        icon.MenuItems.Add(x =>
        {
            x.Text = $"Version 1.0.0";
            x.TextDisabledColor = x.TextColor;
            x.IsDisabled = true;
        });
        icon.MenuItems.Add();
        icon.MenuItems.Add(x =>
        {
            x.Text = "Exit";
            x.Clicked = _ => cts.Cancel();
        });
        */
        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (Exception) { }

        try
        {
            File.Delete(tempPath ?? "");
        }
        catch (Exception) { }
    }

    private static async Task PeriodicAction(Action action, TimeSpan period)
    {
        using (var timer = new PeriodicTimer(period))
        {
            try
            {
                while (await timer.WaitForNextTickAsync())
                {
                    action();
                }
            }
            catch (OperationCanceledException) { }
        }
    }

    [SupportedOSPlatform("windows")]
    private static string? CreateTestIcon(StockIconId id, StockIconOptions options = StockIconOptions.ShellIconSize)
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