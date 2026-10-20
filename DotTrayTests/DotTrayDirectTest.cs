namespace DotTrayTests;

using DotTray;
using DotTray.Direct;
using DotTray.Direct.Items;
using System.Drawing;
using System.Security.Cryptography;

public static class DotTrayDirectTest
{
    public static async Task TestAsync()
    {
        using (var cts = new CancellationTokenSource())
        {
            using (var tempIcon = CreateTestIcon(StockIconId.Application))
            {
                using (var handler = new DirectPopupMenuHandler())
                {
                    using (var icon = await NotifyIcon.RunAsync(tempIcon.FilePath, handler, cts.Token))
                    {
                        icon.Handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "ABC";
                            x.Interacted = args => Console.WriteLine(args);
                        });
                        icon.Handler.MenuItems.Add<MenuItem>(x => x.Text = "DEF");
                        icon.Handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "GHI 🫪";
                            x.FontInfo = new FontInfo
                            {
                                Family = "Segoe UI Emoji",
                                Size = 200f
                            };
                            x.Background = Color.Blue;
                            x.Items.Add<MenuItem>(x => x.Text = "Hi");
                            x.Items.Add<MenuItem>(x =>
                            {
                                x.Text = "This is a test";
                                x.FontInfo = new FontInfo
                                {
                                    Family = "Bahnschrift",
                                    Size = 175f,
                                    Weight = FontWeight.Bold,
                                    Italic = true,
                                    Align = TextAlign.Center
                                };
                                x.Background = Color.Red;
                                x.BackgroundHover = Color.RebeccaPurple;
                                x.Foreground = Color.White;
                                x.ForegroundHover = Color.ForestGreen;
                            });
                            x.Interacted = args => Console.WriteLine(args);
                        });
                        icon.Handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "JKL 🐖";
                            x.FontInfo = new FontInfo
                            {
                                Family = "Arial Black",
                                Size = 120f,
                                Align = TextAlign.Center
                            };
                        });

                        Console.WriteLine("Direct icon is running");

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var span = new byte[3];
                                using (var timer = new PeriodicTimer(TimeSpan.FromSeconds(0.5)))
                                {
                                    while (await timer.WaitForNextTickAsync(cts.Token))
                                    {
                                        RandomNumberGenerator.Fill(span);
                                        icon.Handler.MenuItems.OfType<MenuItem>().SkipLast(1).Last().Background = Color.FromArgb(span[0], span[1], span[2]);
                                    }
                                }
                            }
                            catch (Exception) { }
                        });

                        try
                        {
                            await Task.Delay(Timeout.Infinite, cts.Token);
                        }
                        catch (OperationCanceledException) { }
                    }
                }
            }
        }
    }

    private static TestIcon CreateTestIcon(StockIconId id, StockIconOptions options = StockIconOptions.ShellIconSize)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"temptesticon.ico");

        using (var icon = SystemIcons.GetStockIcon(id, options))
        {
            if (icon is null) throw new FileNotFoundException("Icon not found");

            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                icon.Save(fileStream);
                fileStream.Flush();
            }
        }

        return new TestIcon(tempPath);
    }

    private sealed class TestIcon : IDisposable
    {
        public string FilePath { get; }

        public TestIcon(string filePath) => FilePath = filePath;

        public void Dispose()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                Console.WriteLine("Deleted: " + FilePath);
            }
        }
    }
}