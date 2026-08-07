namespace DotTrayTests;

using AsyncAwaitBestPractices;
using DotTray;
using DotTray.Popup.Default;
using DotTray.Popup.Default.Coloring;
using DotTray.Popup.Default.Items;
using System.Drawing;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[SupportedOSPlatform("windows")]
sealed class Program
{
    static async Task Main()
    {
        Console.WriteLine(Thread.CurrentThread.Name ?? "MainThread");

        var cts = new CancellationTokenSource();

        var tempPath = CreateTestIcon(StockIconId.Error) ?? throw new InvalidOperationException("Icon could not be created");
        var tempPath2 = CreateTestIcon((StockIconId)Random.Shared.Next(0, 141)) ?? throw new InvalidOperationException("Icon could not be created");
        
        var freakyIcon = await NotifyIcon.RunAsync(tempPath, cts.Token);
        var basicIcon = NotifyIcon.Run(tempPath2, cts.Token);

        BuildFreakyMenu(freakyIcon);
        BuildBasicMenu(basicIcon);

        await Task.Delay(5000);

        freakyIcon.Handler.MenuItems.Add<CheckItem>(x =>
        {
            x.Text = "Test";
            x.FontInfo = x.FontInfo with { Size = x.FontInfo.Size * 2 };
        });
        (freakyIcon.Handler.MenuItems[0] as MenuItem)!.Text = "Really long new text, lets see if the resize is correct ;'\"{}[]-_?!";

        PeriodicAction(() =>
        {
            tempPath = CreateTestIcon((StockIconId)Random.Shared.Next(0, 141)) ?? throw new InvalidOperationException("Icon could not be created");

            freakyIcon.SetToolTip(Random.Shared.Next(0, 2) == 0 ? tempPath : null);
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

    private static void BuildBasicMenu(NotifyIcon<DefaultPopupMenuHandler> icon)
    {
        icon.SetToolTip("Basic");

        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Start Application";
        });
        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Actions";

            x.Items.Add<MenuItem>(x =>
            {
                x.Text = "Do something cool";
            });

            x.Items.Add<MenuItem>(x =>
            {
                x.Text = "Do something even cooler";
            });
        });
        icon.Handler.MenuItems.Add<CheckItem>(x =>
        {
            x.Text = "I'm checkable and also very long for testing purpose";
            x.Items.Add<MenuItem>(x => x.Text = "Hello");
            x.Items.Add<SeparatorItem>();
            x.Items.Add<SeparatorItem>();
            x.Items.Add<SeparatorItem>();
            x.Items.Add<MenuItem>(x => x.Text = "Hello");
        });
        icon.Handler.MenuItems.Add<SeparatorItem>();
        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Exit";
        });
    }

    private static void BuildFreakyMenu(NotifyIcon<DefaultPopupMenuHandler> icon)
    {
        icon.SetToolTip("Freaky");
        icon.Handler.SetColor(LinearGradientColor.Random());

        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Background = SolidColor.Transparent;
            x.Foreground = new LinearGradientColor(SolidColor.White, SolidColor.Black, 0f);
            x.Text = "Item No.1";
            x.FontInfo = new FontInfo
            {
                FontFamilyName = "Mistral",
                Size = 100f
            };
            x.Interacted = (args) =>
            {
                if (args.Type is not (ItemInteractionType.MouseLeftUp or ItemInteractionType.KeyboardActivate)) return;
                x.Text = $"Item No. {Random.Shared.Next(10000)}";
                Console.WriteLine("TEXT IS UPDATED");
            };
        });
        icon.Handler.MenuItems.Add<SeparatorItem>();
        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Just some looooooooooooooooooooooooooong text :o";
            x.Background = SolidColor.Red;
            x.BackgroundHover = SolidColor.Blue;
            x.BackgroundDisabled = SolidColor.Green;
            x.Foreground = SolidColor.Green;
            x.ForegroundHover = SolidColor.Red;
            x.ForegroundDisabled = SolidColor.Blue;
        });
        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "-♡👩🏼‍❤️‍👨🏻🐻💩-";
            x.FontInfo = x.FontInfo with { Size = 125f };
        });
        icon.Handler.MenuItems.Add<MenuItem>(x => x.Text = "Empty Submenu :,)");
        icon.Handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Submenu :)";
            x.Background = SolidColor.Red;
            x.Foreground = SolidColor.White;
            x.FontInfo = x.FontInfo with { Size = 100f };
            x.Background = SolidColor.Red;
            x.BackgroundHover = SolidColor.Blue;
            x.BackgroundDisabled = SolidColor.Green;
            x.Interacted = async (args) =>
            {
                if (args.Type is not ItemInteractionType.MouseLeftUp) return;

                x.IsDisabled = true;
                await Task.Delay(2000);
                x.IsDisabled = false;
            };

            x.Items.Add<MenuItem>(x =>
            {
                x.Text = "Submenu Item 1";
                x.Background = SolidColor.White;
                x.Foreground = SolidColor.Black;
                x.FontInfo = x.FontInfo with { Size = 50f };
            });
            x.Items.Add<MenuItem>(x =>
            {
                x.Text = "Submenu Item 2";
                x.Background = SolidColor.White;
                x.Foreground = SolidColor.Black;
                x.FontInfo = x.FontInfo with { Size = 50f };

                x.Items.Add<MenuItem>(x =>
                {
                    x.Text = ":o";
                    x.Background = SolidColor.Black;
                    x.Foreground = SolidColor.White;
                    x.FontInfo = x.FontInfo with { Size = 250f };
                });
                x.Items.Add<MenuItem>(x =>
                {
                    x.Text = ":o";
                    x.Background = SolidColor.Black;
                    x.Foreground = SolidColor.White;
                    x.FontInfo = x.FontInfo with { Size = 25f };
                });
            });
            x.Items.Add<MenuItem>(x =>
            {
                x.Text = "Submenu Item 3";
                x.Background = SolidColor.White;
                x.Foreground = SolidColor.Black;
                x.FontInfo = x.FontInfo with { Size = 50f };
            });
            x.Items.Add<MenuItem>(x =>
            {
                x.Text = "Submenu Item 4";
                x.Background = SolidColor.White;
                x.Foreground = SolidColor.Black;
                x.FontInfo = x.FontInfo with { Size = 50f };
            });
        });
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