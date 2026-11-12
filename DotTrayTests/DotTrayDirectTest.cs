namespace DotTrayTests;

using DotTray;
using DotTray.Direct;
using DotTray.Direct.Coloring;
using DotTray.Direct.Context;
using DotTray.Direct.Imaging;
using DotTray.Direct.Items;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;

public static class DotTrayDirectTest
{
    public static async Task TestAsync()
    {
        TrayTest[] tests =
        [
            new(System.Drawing.StockIconId.Application, ConfigureFullMenu),
            new(System.Drawing.StockIconId.Info, ConfigureSimpleMenu),
            new(System.Drawing.StockIconId.Delete, ConfigureBoundsMenu)
        ];

        using (var cts = new CancellationTokenSource())
        {
            var disposables = new List<IDisposable>();

            try
            {
                foreach (var test in tests)
                {
                    var tempIcon = CreateTestIcon(test.Icon);
                    disposables.Add(tempIcon);

                    var handler = new DirectPopupMenuHandler
                    {
                        Background = LinearGradientBrush.Vertical(Rgba.Blue, Rgba.Green)
                    };
                    disposables.Add(handler);

                    var icon = await NotifyIcon.RunAsync(tempIcon.FilePath, handler, cts.Token);
                    disposables.Add(icon);

                    test.Configure(handler);
                }

                Console.WriteLine($"{tests.Length} direct icon(s) running");

                try
                {
                    await Task.Delay(Timeout.Infinite, cts.Token);
                }
                catch (OperationCanceledException) { }
            }
            finally
            {
                for (var i = disposables.Count - 1; i >= 0; i--) disposables[i].Dispose();
            }
        }
    }

    private static void ConfigureBoundsMenu(DirectPopupMenuHandler handler)
    {
        handler.Background = SolidColorBrush.Black;

        handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Min - MenuItem";
            x.TextInfo = x.TextInfo with
            {
                Size = float.MinValue
            };
        });
        handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Max - MenuItem";
            x.TextInfo = x.TextInfo with
            {
                //Size = float.MaxValue
            };
        });
        handler.MenuItems.Add<SeparatorItem>(x =>
        {
            x.Thickness = int.MinValue;
            x.HorizontalPadding = int.MinValue;
            x.VerticalPadding = int.MinValue;
            x.IsRounded = true;
        });
        handler.MenuItems.Add<CheckItem>(x =>
        {
            x.Text = "Min - CheckTest";
            x.TextInfo = x.TextInfo with
            {
                Size = float.MinValue
            };
        });
        handler.MenuItems.Add<CheckItem>(x =>
        {
            x.Text = "Max - CheckTest";
            x.TextInfo = x.TextInfo with
            {
                //Size = float.MaxValue
            };
        });
        handler.MenuItems.Add<RadioItem>(x =>
        {
            x.Text = "Min - RadioTest";
            x.TextInfo = x.TextInfo with
            {
                Size = float.MinValue
            };
        });
        handler.MenuItems.Add<RadioItem>(x =>
        {
            x.Text = "Max - RadioTest";
            x.TextInfo = x.TextInfo with
            {
                //Size = float.MaxValue
            };
        });
    }

    private static void ConfigureSimpleMenu(DirectPopupMenuHandler handler)
    {
        handler.Background = SolidColorBrush.Black;

        handler.MenuItems.Add<ImageItem>();

        var group = new RadioItemGroup();
        group.SelectionChanged += args => Console.WriteLine((args?.Text ?? "<NULL>") + " is now selected");

        handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Add/Remove Radio Item";
            x.TextInfo = x.TextInfo with
            {
                Align = TextAlign.Center,
                Stretch = FontStretch.Expanded,
                Weight = FontWeight.Bold
            };
            x.Interacted = args =>
            {
                args.KeepMenuOpen = true;
                if (args.Type is ItemInteractionType.MouseLeftUp)
                {
                    handler.MenuItems.Insert<RadioItem>(handler.MenuItems.Count - 1, x =>
                    {
                        x.Text = "Radio " + Random.Shared.Next(100_000).ToString();
                        x.Group = group;
                        x.Interacted = args => args.KeepMenuOpen = true;
                    });
                }
                else if (args.Type is ItemInteractionType.MouseRightUp)
                {
                    foreach (var item in handler.MenuItems.OfType<RadioItem>())
                    {
                        group.Remove(item);
                        handler.MenuItems.Remove(item);
                    }
                }
            };
        });

        handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "Clear Selection";
            x.TextInfo = x.TextInfo with
            {
                Align = TextAlign.Center,
                Stretch = FontStretch.Expanded,
                Weight = FontWeight.SemiBold,
                Style = FontStyle.Italic
            };
            x.Interacted = args =>
            {
                args.KeepMenuOpen = true;
                if (args.Type is not ItemInteractionType.MouseLeftUp) return;
                
                group.ClearSelection();
            };
        });
    }

    private static void ConfigureFullMenu(DirectPopupMenuHandler handler)
    {
        handler.MenuItems.Add<CheckItem>(x =>
        {
            x.Text = "Eternity";
            x.TextInfo = x.TextInfo with
            {
                //Font = new UserFont(@"C:\pixelify.ttf"),
                Font = new UserFont(@"C:\Users\leschi\Downloads\honk.ttf"),
                Size = 75f
            };
            x.Interacted = args =>
            {
                if (args.Type is ItemInteractionType.MouseLeftUp) args.KeepMenuOpen = Random.Shared.Next(0, 2) == 1;

                Console.WriteLine(args);
            };
        });
        handler.MenuItems.Add<SeparatorItem>(x =>
        {
            x.Background = new SolidColorBrush(Rgba.White with { A = 64 });
            x.Foreground = SolidColorBrush.White;
            x.HorizontalPadding = 8;
            x.VerticalPadding = 4;
            x.Thickness = 4;
        });
        handler.MenuItems.Add<MenuItem>(x =>
        {
            x.Text = "DEF";
            x.TextInfo = x.TextInfo with { Size = x.TextInfo.Size * 2f };
            x.Items.Add<CheckItem>(x => x.Text = "Hello");
            x.Items.Add<CheckItem>(x =>
            {
                x.Text = "Checkable";
                x.TextInfo = x.TextInfo with { Size = x.TextInfo.Size * 3f };
                x.Interacted = args =>
                {
                    Console.WriteLine(x.IsChecked);
                    args.KeepMenuOpen = true;
                };
            });
            x.Items.Add<CheckItem>(x =>
            {
                x.Text = "Checkable²";
                x.TextInfo = x.TextInfo with { Size = x.TextInfo.Size * 6f };
                x.Interacted = args => args.KeepMenuOpen = true;
            });
        });
        handler.MenuItems.Add<SeparatorItem>();
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
                //Font = new UserFont(@"C:\NotoColorEmoji-Regular.ttf"),
                Font = SystemFont.Default,
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
        handler.MenuItems.Add<SeparatorItem>(x =>
        {
            x.Foreground = SolidColorBrush.Red;
            x.IsRounded = false;
            x.Thickness = 8;
            x.Interacted = args => Console.WriteLine("This should never be fired!");
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
    }

    private static TestIcon CreateTestIcon(System.Drawing.StockIconId id, System.Drawing.StockIconOptions options = System.Drawing.StockIconOptions.ShellIconSize)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"temptesticon_{id}_{Guid.NewGuid():N}.ico");

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

    private sealed record TrayTest(System.Drawing.StockIconId Icon, Action<DirectPopupMenuHandler> Configure);
}

file sealed class ImageItem : MenuItemBase
{
    private readonly DirectImage _image;
    private readonly Dim _actualSize;

    protected override bool IgnoreInteraction => true;

    public ImageItem()
    {

        //_image = DirectImage.FromFile(@"C:\testimg.jpg");
        _image = DirectImage.FromFile(@"C:\Users\leschi\Downloads\le kirky.png");
        _actualSize = new Dim(_image.Dimensions.Width / 2, _image.Dimensions.Height / 2);
    }

    protected override Dim Measure(MeasuringContext context) => _actualSize;
    protected override void Draw(DrawingContext context)
    {
        var bounds = context.ItemBounds;
        var x = bounds.X + (bounds.Width - _actualSize.Width) / 2;

        context.DrawImage(_image, new Rect(new Pos(x, bounds.Y), _actualSize), new ImageOptions
        {
            Opacity = 0.9f,
            InterpolationMode = InterpolationMode.HighQualityCubic
        });
    }
}