namespace DotTrayTests;

using AsyncAwaitBestPractices;
using DotTray;
using DotTray.Popup.Default;
using DotTray.Popup.Default.Coloring;
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

        /*
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
        */

        icon.Handler.SetColor(new LinearGradientColor
        {
            Start = new SolidColor(0, 0, 255),
            End = new SolidColor(0, 255, 255)
        });

        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Background = SolidColor.Transparent;
            x.Foreground = new LinearGradientColor(SolidColor.White, SolidColor.Black, 0f);
            x.Text = "Item No.1";
            x.FontInfo = new FontInfo
            {
                FontFamilyName = "Mistral",
                Size = 200f
            };
        });
        icon.Handler.MenuItems.Add<SeparatorItem>();
        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Just some looooooooooooooooooooooooooong text :o";
        });
        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "-♡👩🏼‍❤️‍👨🏻🐻💩-";
            x.FontInfo = x.FontInfo with { Size = 125f };
        });

        icon2.Handler.MenuItems.Add<SeparatorItem>(x => { });
        
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