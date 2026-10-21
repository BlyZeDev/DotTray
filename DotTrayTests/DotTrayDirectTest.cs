namespace DotTrayTests;

using DotTray;
using DotTray.Direct;
using DotTray.Direct.Coloring;
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
                    handler.Background = new SolidColorBrush(Rgba.Black with { A = 200 });

                    using (var icon = await NotifyIcon.RunAsync(tempIcon.FilePath, handler, cts.Token))
                    {
                        handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "ABC";
                            x.Interacted = args => Console.WriteLine(args);
                        });
                        handler.MenuItems.Add<MenuItem>(x => x.Text = "DEF");
                        handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "GHI 🫪";
                            x.FontInfo = new FontInfo
                            {
                                Family = "Segoe UI Emoji",
                                Size = 150f
                            };
                            x.Items.Add<MenuItem>(x => x.Text = "Hi");
                            x.Items.Add<MenuItem>(x =>
                            {
                                x.Text = "This is a test";
                                x.FontInfo = new FontInfo
                                {
                                    Family = "Bahnschrift",
                                    Size = 125f,
                                    Weight = FontWeight.Bold,
                                    Italic = true,
                                    Align = TextAlign.Center
                                };
                            });
                            x.Interacted = args => Console.WriteLine(args);
                        });
                        handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "JKL 🐖";
                            x.FontInfo = new FontInfo
                            {
                                Family = "Arial Black",
                                Size = 100f,
                                Align = TextAlign.Center
                            };
                        });

                        Console.WriteLine("Direct icon is running");

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