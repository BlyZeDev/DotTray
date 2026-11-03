namespace DotTrayTests;

using DotTray;
using DotTray.Direct;
using DotTray.Direct.Coloring;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;

public static class DotTrayDirectTest
{
    public static async Task TestAsync()
    {
        using (var cts = new CancellationTokenSource())
        {
            using (var tempIcon = CreateTestIcon(System.Drawing.StockIconId.Application))
            {
                using (var handler = new DirectPopupMenuHandler())
                {
                    handler.Background = new SolidColorBrush(Rgba.Black with { A = 200 });
                    handler.Background = LinearGradientBrush.Vertical(Rgba.Blue, Rgba.Green);

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
                            x.Text = "Qualle🪼";
                            x.Background = new SolidColorBrush(Rgba.White);
                            x.Foreground = LinearGradientBrush.FromAngle(
                                [
                                    new GradientStop(Rgba.Red, 0f),
                                    new GradientStop(Rgba.Green, 1f / 3f),
                                    new GradientStop(Rgba.Blue, 2f / 3f),
                                    new GradientStop(Rgba.Black, 1f)
                                ], 0f);
                            x.TextInfo = new TextInfo
                            {
                                Font = new UserFont(@"C:\NotoColorEmoji-Regular.ttf"),
                                Size = 150f
                            };
                            x.Items.Add<MenuItem>(x =>
                            {
                                x.Text = "Hi";
                                x.TextInfo = new TextInfo
                                {
                                    Font = SystemFont.Default,
                                    Size = 250f
                                };
                                x.Background = new RadialGradientBrush(
                                    [
                                        new GradientStop(new Rgba(254, 225, 64), 0f),
                                        new GradientStop(new Rgba(250, 112, 154), 0.5f),
                                        new GradientStop(new Rgba(127, 0, 255), 1f)
                                    ], new PosF(0.3f, 0.3f), new PosF(0.85f, 0.85f), new PosF(-0.1f, -0.1f));
                                x.BackgroundHover = x.Background;
                            });
                            x.Items.Add<MenuItem>(x =>
                            {
                                x.Text = "This is a test";
                                x.TextInfo = new TextInfo
                                {
                                    Font = new SystemFont("Bahnschrift"),
                                    Size = 125f,
                                    Weight = FontWeight.Bold,
                                    Style = FontStyle.Oblique,
                                    Align = TextAlign.Center
                                };
                            });
                            x.Interacted = args => Console.WriteLine(args);
                        });
                        handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "JKL 🐖";
                            x.TextInfo = new TextInfo
                            {
                                Font = new SystemFont("Arial"),
                                Size = 100f,
                                Weight = FontWeight.Light,
                                Align = TextAlign.Center
                            };
                        });
                        handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "HHH";
                            x.Background = new SolidColorBrush(Rgba.White);
                            x.Foreground = LinearGradientBrush.FromAngle(
                                [
                                    new GradientStop(Rgba.Red, 0f),
                                    new GradientStop(Rgba.Green, 1f / 3f),
                                    new GradientStop(Rgba.Blue, 2f / 3f),
                                    new GradientStop(Rgba.Black, 1f)
                                ], 180f);
                            x.TextInfo = new TextInfo
                            {
                                Font = new SystemFont("Arial"),
                                Size = 100f,
                                Weight = FontWeight.Light,
                                Align = TextAlign.Center
                            };
                        });
                        handler.MenuItems.Add<MenuItem>(x =>
                        {
                            x.Text = "🪼🔥 Élite 🔥🪼";
                            x.ForegroundHover = LinearGradientBrush.FromAngle(
                                [
                                    new GradientStop(new Rgba(255, 235, 160), 0f),
                                    new GradientStop(new Rgba(240, 190, 60), 0.25f),
                                    new GradientStop(new Rgba(200, 130, 20), 0.5f),
                                    new GradientStop(new Rgba(150, 80, 10), 0.75f),
                                    new GradientStop(new Rgba(90, 40, 5), 1f)
                                ], 135f);
                            x.TextInfo = new TextInfo
                            {
                                Font = new SystemFont("Algerian"),
                                Size = 111f,
                                Align = TextAlign.Center,
                                Language = new LanguageTag("fr-FR")
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

    private static TestIcon CreateTestIcon(System.Drawing.StockIconId id, System.Drawing.StockIconOptions options = System.Drawing.StockIconOptions.ShellIconSize)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"temptesticon.ico");

        using (var icon = System.Drawing.SystemIcons.GetStockIcon(id, options))
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