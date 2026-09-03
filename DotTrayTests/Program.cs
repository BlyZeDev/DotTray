using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]

namespace DotTrayTests;

using AsyncAwaitBestPractices;
using DotTray;
using DotTray.Popup.Default;
using DotTray.Popup.Default.Coloring;
using DotTray.Popup.Default.Context;
using DotTray.Popup.Default.Items;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

sealed class Program
{
    static async Task Main()
    {
        Console.WriteLine(Thread.CurrentThread.Name ?? "MainThread");

        var cts = new CancellationTokenSource();

        var tempPath = CreateTestIcon(StockIconId.Error) ?? throw new InvalidOperationException("Icon could not be created");
        var tempPath2 = CreateTestIcon((StockIconId)Random.Shared.Next(0, 141)) ?? throw new InvalidOperationException("Icon could not be created");
        
        using var freakyIcon = await NotifyIcon.RunAsync(tempPath, cts.Token);
        using var basicIcon = NotifyIcon.Run(tempPath2, cts.Token);

        BuildFreakyMenu(freakyIcon);
        BuildBasicMenu(basicIcon);

        using var icon = await NotifyIcon.RunAsync(CreateTestIcon(StockIconId.Find), cts.Token);

        icon.Handler.MenuItems.Add<SearchBarItem>(x =>
        {
            x.TextChanged = text =>
            {
                foreach (var item in icon.Handler.MenuItems.OfType<MenuItem>().ToArray())
                {
                    if (!string.IsNullOrEmpty(text) && item.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                    {
                        item.Background = SolidColor.Red;
                        item.Foreground = SolidColor.White;
                        icon.Handler.MenuItems.Move(icon.Handler.MenuItems.IndexOf(item), 1);
                    }
                    else
                    {
                        item.Background = SolidColor.White;
                        item.Foreground = SolidColor.Black;
                    }
                }
            };
        });
        icon.Handler.MenuItems.Add<MenuItem>(x => x.Text = "Fortnite");
        icon.Handler.MenuItems.Add<MenuItem>(x => x.Text = "Counter-Strike");
        icon.Handler.MenuItems.Add<MenuItem>(x => x.Text = "Among Us");
        icon.Handler.MenuItems.Add<MenuItem>(x => x.Text = "Overwatch");
        icon.Handler.MenuItems.Add<MenuItem>(x => x.Text = "Minecraft");
        icon.Handler.MenuItems.Add<MenuItem>(x => x.Text = "Cuphead");

        await Task.Delay(5000);

        freakyIcon.Handler.MenuItems.Add<CheckItem>(x =>
        {
            x.Text = "Test";
            x.Foreground = SolidColor.Random();
            x.FontInfo = x.FontInfo with { Size = x.FontInfo.Size * 2 };
        });

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

        icon.Handler.MenuItems.Add<ImageItem>();
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

file sealed class ImageItem : MenuItemBase
{
    private readonly ImageSource _image;

    protected override bool IgnoreInteraction => true;

    public ImageItem()
    {
        var sw = Stopwatch.StartNew();
        _image = ImageSource.FromHBitmap(SystemIcons.GetStockIcon((StockIconId)Random.Shared.Next(141), 256).ToBitmap().GetHbitmap());
        sw.Stop();
        Console.WriteLine("Init: " + sw.ElapsedMilliseconds + "ms");
    }

    protected override DotTray.Primitives.Size Measure(MeasuringContext context)
    {
        return new DotTray.Primitives.Size(_image.Size.Width, _image.Size.Height);
    }

    protected override void Draw(DrawingContext context)
    {
        var sw = Stopwatch.StartNew();
        context.DrawImage(_image);
        context.DrawImageRect(context.ItemBounds with
        {
            X = context.ItemBounds.Width / 2 - _image.Size.Width / 2,
            Width = _image.Size.Width,
            Height = _image.Size.Height
        }, _image);
        sw.Stop();
        Console.WriteLine("Draw: " + sw.ElapsedMilliseconds + "ms");
    }
}

file sealed class SearchBarItem : MenuItemBase
{
    private const char Caret = '▎';
    private const int Padding = 8;
    private const int CaretBlinkMs = 500;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private const int VK_BACK = 0x08;
    private const int VK_ESCAPE = 0x1B;
    private const int VK_RETURN = 0x0D;
    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12;
    private const int VK_CAPITAL = 0x14;
    private const int VK_SPACE = 0x20;
    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;
    private const int VK_LSHIFT = 0xA0;
    private const int VK_RSHIFT = 0xA1;
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;
    private const int VK_LMENU = 0xA4;
    private const int VK_RMENU = 0xA5;

    private static readonly int[] ModifierVks =
    [
        VK_SHIFT, VK_CONTROL, VK_MENU,
        VK_LSHIFT, VK_RSHIFT, VK_LCONTROL, VK_RCONTROL, VK_LMENU, VK_RMENU
    ];

    private readonly ImageSource _searchIcon;
    private readonly FontInfo _fontInfo;
    private readonly StringBuilder _text;

    private readonly HookProc _hookProc;
    private nint _hookHandle;
    private bool _focused;

    private Timer? _caretTimer;
    private bool _caretVisible;

    public string Text => _text.ToString();
    public Action<string>? TextChanged { get; set; }

    public SearchBarItem()
    {
        _searchIcon = ImageSource.FromHIcon(SystemIcons.GetStockIcon(StockIconId.Find, 48).Handle);
        _fontInfo = new FontInfo("Segoe UI Emoji", 24f, FontAlignment.Near);
        _text = new StringBuilder();
        _hookProc = KeyboardHookCallback;
    }

    protected override void Initialize()
    {
        _text.Clear();
        SetFocused(false);
    }

    protected override DotTray.Primitives.Size Measure(MeasuringContext context)
    {
        var measuredText = (_text.Length == 0 && !_focused ? "Search..." : Text) + Caret;
        var measured = context.MeasureText(measuredText, _fontInfo);

        var width = Math.Max(_searchIcon.Size.Width * 4, Padding + _searchIcon.Size.Width + Padding + (int)MathF.Ceiling(measured.Width) + Padding);

        return new DotTray.Primitives.Size(width, _searchIcon.Size.Height + Padding * 2);
    }

    protected override void Draw(DrawingContext context)
    {
        var bounds = context.ItemBounds;

        context.Fill(new SolidColor(200, 200, 200));

        context.DrawImageRect(bounds with
        {
            X = bounds.X + Padding,
            Y = bounds.Y + Padding,
            Width = _searchIcon.Size.Width,
            Height = _searchIcon.Size.Height
        }, _searchIcon);

        var caret = _focused && _caretVisible ? Caret : char.MinValue;
        var displayText = _text.Length == 0 && !_focused ? "Search..." : Text + caret;

        context.WriteRect(bounds with
        {
            X = bounds.X + Padding + _searchIcon.Size.Width + Padding,
            Width = bounds.Width - _searchIcon.Size.Width - Padding * 2
        }, displayText, _fontInfo, SolidColor.Black);
    }

    protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        switch (args.Type)
        {
            case ItemInteractionType.MouseLeftDown:
            case ItemInteractionType.MouseLeftUp:
            case ItemInteractionType.KeyboardActivate:
                SetFocused(true);
                args.KeepMenuOpen = true;
                break;
        }
    }

    protected override void Cleanup()
    {
        RemoveHook();
        StopCaretBlink();
    }

    private void SetFocused(bool focused)
    {
        if (_focused == focused) return;
        _focused = focused;

        if (_focused)
        {
            InstallHook();
            StartCaretBlink();
        }
        else
        {
            RemoveHook();
            StopCaretBlink();
        }

        Update();
    }

    private void StartCaretBlink()
    {
        _caretVisible = true;
        _caretTimer?.Dispose();
        _caretTimer = new Timer(_ =>
        {
            _caretVisible = !_caretVisible;
            Update();
        }, null, CaretBlinkMs, CaretBlinkMs);
    }

    private void StopCaretBlink()
    {
        _caretTimer?.Dispose();
        _caretTimer = null;
        _caretVisible = false;
    }

    private void InstallHook()
    {
        if (_hookHandle != nint.Zero) return;

        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;

        _hookHandle = SetWindowsHookEx(
            WH_KEYBOARD_LL,
            _hookProc,
            GetModuleHandle(curModule.ModuleName!),
            0);
    }

    private void RemoveHook()
    {
        if (_hookHandle == nint.Zero) return;
        UnhookWindowsHookEx(_hookHandle);
        _hookHandle = nint.Zero;
    }

    private nint KeyboardHookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
        {
            var vkCode = Marshal.ReadInt32(lParam);
            var scanCode = (uint)Marshal.ReadInt32(lParam, 4);

            bool winDown = (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0
                        || (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0;

            if (winDown)
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);

            switch (vkCode)
            {
                case VK_BACK:
                    if (_text.Length > 0)
                    {
                        _text.Length--;
                        TextChanged?.Invoke(Text);
                        Update();
                    }
                    return 1;

                case VK_ESCAPE:
                    SetFocused(false);
                    return CallNextHookEx(_hookHandle, nCode, wParam, lParam);

                case VK_RETURN:
                    SetFocused(false);
                    return 1;

                default:
                    bool ctrlDown = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
                    bool altDown = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
                    bool altGr = ctrlDown && altDown;

                    if ((ctrlDown || altDown) && !altGr)
                        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);

                    var ch = VirtualKeyToChar(vkCode, scanCode);
                    if (ch != '\0' && !char.IsControl(ch))
                    {
                        _text.Append(ch);
                        TextChanged?.Invoke(Text);
                        Update();
                        return 1;
                    }
                    break;
            }
        }

        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private static char VirtualKeyToChar(int vkCode, uint scanCode)
    {
        var keyboardState = new byte[256];

        foreach (var vk in ModifierVks)
        {
            if ((GetAsyncKeyState(vk) & 0x8000) != 0)
                keyboardState[vk] = 0x80;
        }

        if ((GetKeyState(VK_CAPITAL) & 0x1) != 0)
            keyboardState[VK_CAPITAL] = 0x01;

        var foregroundWindow = GetForegroundWindow();
        var foregroundThreadId = GetWindowThreadProcessId(foregroundWindow, out _);
        var hkl = GetKeyboardLayout(foregroundThreadId);

        var buffer = new StringBuilder(8);
        var result = ToUnicodeEx((uint)vkCode, scanCode, keyboardState, buffer, buffer.Capacity, 0, hkl);

        if (result > 0)
            return buffer[0];

        if (result < 0)
        {
            buffer.Clear();
            var flush = ToUnicodeEx(VK_SPACE, 0, keyboardState, buffer, buffer.Capacity, 0, hkl);
            return flush > 0 ? buffer[0] : '\0';
        }

        return '\0';
    }

    private delegate nint HookProc(int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern nint GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll")]
    private static extern int ToUnicodeEx(
        uint wVirtKey, uint wScanCode, byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff,
        int cchBuff, uint wFlags, nint dwhkl);
}