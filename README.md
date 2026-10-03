 <div align="center">

<img src="https://github.com/BlyZeDev/DotTray/blob/master/icon.png" alt="DotTray Icon" width="128" height="128" />

# DotTray

**A lightweight, customizable and NativeAOT-ready Windows notification icon for .NET 10**

[![NuGet](https://img.shields.io/nuget/v/DotTray?style=for-the-badge&logo=nuget&logoColor=white&logoSize=auto&label=NUGET%20Package&labelColor=%23101010&color=white)](https://www.nuget.org/packages/DotTray)

</div>

---

## Table of Contents

- [Installation](#installation)
- [1. Quickstart](#1-quickstart)
- [2. Features](#2-features)
  - [Highlights](#highlights)
  - [Creating an icon](#creating-an-icon)
  - [The default popup menu](#the-default-popup-menu)
  - [Tooltips and visibility](#tooltips-and-visibility)
  - [Balloon notifications](#balloon-notifications)
  - [Interaction events](#interaction-events)
  - [Lifetime and cleanup](#lifetime-and-cleanup)
  - [Complete example](#complete-example)
- [3. Extension](#3-extension)
  - [Option A: `INotifyIconHandler`](#option-a-inotifyiconhandler-complete-control)
  - [Option B: `PopupMenuHandler`](#option-b-popupmenuhandler-menu-outline)
  - [Which one should I choose?](#which-one-should-i-choose)
- [Requirements](#requirements)

---

## Installation

```shell
dotnet add package DotTray
```

or via the Package Manager Console:

```powershell
Install-Package DotTray
```

---

## 1. Quickstart

The absolute minimum: show an icon in the system tray with a tiny popup menu that has an **Exit** entry.

> [!NOTE]
> The icon source must be a path to an **`.ico`** file (or an existing icon handle, see [Creating an icon](#creating-an-icon)).

```csharp
using DotTray;
using DotTray.Items;

using (var cts = new CancellationTokenSource())
{
    // 1. Create the handler that provides the popup menu
    using (var handler = new DefaultPopupMenuHandler())
    {
        handler.Items.Add(new MenuItem
        {
            Text = "Exit",
            Clicked = _ => cts.Cancel()
        });

        // 2. Show the icon
        using (var icon = NotifyIcon.Run("app.ico", handler, cts.Token))
        {
            icon.SetToolTip("My first tray icon");

            // 3. Keep the app alive until "Exit" is clicked
            cts.Token.WaitHandle.WaitOne();
        }
    }
}
```

That's it. Left- or right-click the icon to open the menu.

> [!IMPORTANT]
> The icon runs on its own background thread. If your `Main` method returns, the process ends and the icon disappears, so make sure your app stays alive (as done above with `WaitOne()`, or with a console loop, a hosted service, etc.).

---

## 2. Features

### Highlights

| Feature | Description |
| --- | --- |
| **NativeAOT compatible** | The library is marked with `IsAotCompatible`. It works with trimming and `PublishAot` without reflection or runtime code generation. |
| **Lightweight** | Talks directly to the Win32 shell API. No WinForms, no WPF, no other dependencies. |
| **Live menu updates** | Change an item's text, disabled state, or checked state, or add/remove items while the menu is open, and it refreshes instantly. |
| **Dark mode** | The native popup menu opts into the Windows dark mode, following the system theme. |
| **Balloon notifications** | Show balloon/toast style messages with title, message, icon, and optional sound. |
| **Tooltips** | Set and update the hover tooltip at any time. |
| **Show / Hide** | Toggle the visibility of the icon without destroying it. |
| **Self-healing** | The icon is automatically restored when Explorer restarts (taskbar re-creation) or the PC resumes from sleep. |
| **Multiple icons** | Run as many icons as you want side by side, each with its own handler. |
| **Fully extensible** | Replace the menu entirely by implementing `INotifyIconHandler` or `PopupMenuHandler`. |
| **Async & sync API** | `NotifyIcon.Run(...)` and `NotifyIcon.RunAsync(...)`. |
| **Cancellation support** | Stop an icon at any time using a `CancellationToken`. |

### Creating an icon

```csharp
// Blocking until the icon is ready
NotifyIcon<DefaultPopupMenuHandler> icon = NotifyIcon.Run("app.ico", handler, cancellationToken);

// Asynchronous variant
NotifyIcon<DefaultPopupMenuHandler> icon = await NotifyIcon.RunAsync("app.ico", handler, cancellationToken);
```

`IconSource` converts **implicitly** from two kinds of values:

| Source | Example | Notes |
| --- | --- | --- |
| `string` (path) | `"C:\\path\\to\\app.ico"` | Must point to an existing `.ico` file, otherwise `ArgumentException` or `FileNotFoundException` is thrown. |
| `nint` (icon handle, `HICON`) | `myHIcon` | The handle is copied. **You remain responsible** for destroying your own handle. |

> [!TIP]
> For the best result, use an `.ico` file that contains a **16x16 and/or 32x32** variant with a **32-bit color depth including an alpha channel**.

### The default popup menu

`DefaultPopupMenuHandler` renders a native Win32 popup menu when the icon is clicked (left or right). Its content is the `Items` collection, an `ObservableCollection<ItemBase>`.

| Item | Description |
| --- | --- |
| `MenuItem` | A normal clickable entry. |
| `CheckItem` | A clickable entry with a check mark. The check state is **toggled automatically** on click. |
| `SubmenuItem` | An entry that opens a nested menu. Has its own `Items` collection (nesting is unlimited). |
| `SeparatorItem.Instance` | A horizontal separator line. |

`MenuItem`, `CheckItem` and `SubmenuItem` share these properties:

| Property | Description |
| --- | --- |
| `Text` | The displayed text. |
| `IsDisabled` | Grays out the item and makes it unclickable. |
| `Clicked` | `Action<T>` invoked when the item is clicked. The item itself is passed as argument. |
| `IsChecked` | *(`CheckItem` only)* The current check state. |

```csharp
using DotTray;
using DotTray.Items;

using (var handler = new DefaultPopupMenuHandler())
{
    // Normal item
    var open = new MenuItem
    {
        Text = "Open",
        Clicked = item => Console.WriteLine("Open clicked!")
    };

    // Check item, IsChecked is toggled automatically before Clicked is invoked
    var autoStart = new CheckItem
    {
        Text = "Start with Windows",
        IsChecked = true,
        Clicked = item => Console.WriteLine($"Auto start is now {item.IsChecked}")
    };

    // Disabled item
    var comingSoon = new MenuItem { Text = "Coming soon...", IsDisabled = true };

    // Submenu
    var more = new SubmenuItem { Text = "More" };
    more.Items.Add(new MenuItem { Text = "About", Clicked = _ => Console.WriteLine("About") });
    more.Items.Add(SeparatorItem.Instance);
    more.Items.Add(new MenuItem { Text = "Help", Clicked = _ => Console.WriteLine("Help") });

    handler.Items.Add(open);
    handler.Items.Add(autoStart);
    handler.Items.Add(comingSoon);
    handler.Items.Add(SeparatorItem.Instance);
    handler.Items.Add(more);
}
```

#### Changing the menu at runtime

Items can be changed whenever you like, even while the menu is currently open. The menu is rebuilt and reopened in place.

```csharp
open.Text = "Open (3 new)";    // updates the text
comingSoon.IsDisabled = false; // enables the item
autoStart.IsChecked = false;   // unchecks the item
menu.Items.Remove(open);       // removes the item
```

> [!NOTE]
> `Clicked` callbacks (and the `Interacted` event) are raised on the icon's **background STA thread**. If you need to touch a UI framework, marshal back to your UI thread. Avoid long-running work in the callback, run it on another thread or `Task` instead.

### Tooltips and visibility

```csharp
icon.SetToolTip("Hello from the tray!"); // set or change the tooltip
icon.SetToolTip(null); // remove the tooltip

icon.Hide(); // hide the icon (it keeps running)
icon.Show(); // show it again

Console.WriteLine(icon.IsVisible); // current visibility
```

Texts longer than the maximum Windows tooltip length are truncated automatically.

### Balloon notifications

Balloon notifications are shown next to the icon (Windows displays them as toast notifications).

```csharp
icon.ShowBalloon(new BalloonNotification
{
    Title = "Download finished",
    Message = "Your file is ready.",
    Icon = BalloonNotificationIcon.Info,
    NoSound = false
});
```

| Property | Description |
| --- | --- |
| `Title` | *(required)* The title. Truncated to the Windows limit. |
| `Message` | *(required)* The message. Truncated to the Windows limit. |
| `Icon` | `None` (default), `Info`, `Warning`, `Error`, or `User` (shows **your tray icon** as the large balloon icon). |
| `NoSound` | `true` to suppress the notification sound. Default is `false`. |

### Interaction events

Every interaction with the icon (or its balloons) is reported through the `Interacted` event:

```csharp
icon.Interacted += args =>
{
    Console.WriteLine($"{args.Type} at {args.MousePosition.X}, {args.MousePosition.Y}");

    if (args.Type is IconInteractionType.BalloonUserClick)
    {
        Console.WriteLine("The user clicked the balloon!");
    }
};
```

`MousePosition` contains the cursor position in **screen coordinates**. The available `IconInteractionType` values are:

| Category | Values |
| --- | --- |
| **Mouse** | `MouseMove`, `LeftButtonDown`, `LeftButtonUp`, `LeftButtonDoubleClick`, `RightButtonDown`, `RightButtonUp`, `MiddleButtonDown`, `MiddleButtonUp` |
| **Selection / Menu** | `Select`, `KeySelect`, `ContextMenu` |
| **Balloons** | `BalloonShow`, `BalloonHide`, `BalloonTimeout`, `BalloonUserClick` |
| **Rich popup (hover)** | `PopupOpen`, `PopupClose` |

### Lifetime and cleanup

- `NotifyIcon<THandler>` implements `IDisposable`. Disposing removes the icon from the tray and stops its thread.
- Cancelling the `CancellationToken` passed to `Run` / `RunAsync` also removes the icon.
- `NotifyIcon.TotalIcons` tells you how many icons are currently running.
- Each icon has a unique `Id` (`Guid`).
- `icon.Handler` gives you access to the handler you passed in, so you can modify the menu later:

```csharp
icon.Handler.Items.Add(new MenuItem { Text = "Added later" });
```

> [!WARNING]
> Do **not** call `Dispose()` on an icon from inside its own `Clicked` / `Interacted` callback. Those run on the icon's own thread, which `Dispose()` waits for. Cancel the `CancellationToken` instead (as done in the quickstart).

### Complete example

```csharp
using DotTray;
using DotTray.Items;

using (var cts = new CancellationTokenSource())
{
    using (var handler = new DefaultPopupMenuHandler())
    {
        var status = new MenuItem { Text = "Status: Idle", IsDisabled = true };
        var notifications = new CheckItem { Text = "Notifications", IsChecked = true };

        var test = new MenuItem { Text = "Send test balloon" };

        var settings = new SubmenuItem { Text = "Settings" };
        settings.Items.Add(new MenuItem { Text = "General" });
        settings.Items.Add(new MenuItem { Text = "Advanced" });

        handler.Items.Add(status);
        handler.Items.Add(SeparatorItem.Instance);
        handler.Items.Add(notifications);
        handler.Items.Add(test);
        handler.Items.Add(settings);
        handler.Items.Add(SeparatorItem.Instance);
        handler.Items.Add(new MenuItem { Text = "Exit", Clicked = _ => cts.Cancel() });

        using (var icon = await NotifyIcon.RunAsync("app.ico", handler, cts.Token))
        {
            icon.SetToolTip("DotTray demo");

            test.Clicked = _ =>
            {
                status.Text = "Status: Working...";

                if (notifications.IsChecked)
                {
                    icon.ShowBalloon(new BalloonNotification
                    {
                        Title = "DotTray",
                        Message = "This is a test balloon.",
                        Icon = BalloonNotificationIcon.User
                    });
                }
            };

            icon.Interacted += args =>
            {
                if (args.Type is IconInteractionType.BalloonUserClick)
                {
                    status.Text = "Status: Idle";
                }
            };

            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (OperationCanceledException) { }
        }
    }
}
```

---

## 3. Extension

The default popup menu is just one possible handler. Every `NotifyIcon<THandler>` delegates its interactions to a handler, and you can write your own in two ways:

```
INotifyIconHandler ← full control, one method for every interaction
  └── PopupMenuHandler ← ready-made outline for popup menus / flyouts
        └── DefaultPopupMenuHandler ← the built-in native Win32 menu
```

### Option A: `INotifyIconHandler` (complete control)

Implement `INotifyIconHandler` if you want to decide **everything** yourself. It has a single method that is invoked for every interaction (the same ones that raise `Interacted`):

```csharp
public interface INotifyIconHandler
{
    void HandleInteraction<THandler>(NotifyIcon<THandler> owner, NotifyIconInteractedEventArgs args) where THandler : class, INotifyIconHandler;
}
```

**Example:** a handler without any menu. A double click shows a balloon, a middle click toggles the tooltip, and a right click exits the app.

```csharp
using DotTray;

public sealed class QuickActionHandler : INotifyIconHandler
{
    private readonly CancellationTokenSource _exit;
    private int _count;

    public QuickActionHandler(CancellationTokenSource exit) => _exit = exit;

    public void HandleInteraction<THandler>(NotifyIcon<THandler> owner, NotifyIconInteractedEventArgs args)
        where THandler : class, INotifyIconHandler
    {
        switch (args.Type)
        {
            case IconInteractionType.LeftButtonDoubleClick:
                _count++;
                owner.ShowBalloon(new BalloonNotification
                {
                    Title = "Double click",
                    Message = $"You double-clicked {_count} time(s).",
                    Icon = BalloonNotificationIcon.Info
                });
                break;

            case IconInteractionType.MiddleButtonUp:
                owner.SetToolTip(owner.ToolTip is null ? "Hello!" : null);
                break;

            case IconInteractionType.ContextMenu:
                _exit.Cancel();
                break;
        }
    }
}
```

Usage:

```csharp
using (var cts = new CancellationTokenSource())
{
    using (var icon = NotifyIcon.Run("app.ico", new QuickActionHandler(cts), cts.Token))
    {
        cts.Token.WaitHandle.WaitOne();
    }
}
```

### Option B: `PopupMenuHandler` (menu outline)

Inherit from `PopupMenuHandler` if your icon should open some kind of popup (a custom menu, a flyout window, a WPF/WinForms/WinUI/Avalonia window...). The base class already maps the raw interactions for you:

| Method | Called when | Required |
| --- | --- | --- |
| `Show(owner, mousePosition)` | The icon is selected (left click / keyboard select). | **Yes** (`abstract`) |
| `ShowContext(owner, mousePosition)` | A context menu is requested (right click / menu key). | **Yes** (`abstract`) |
| `ShowToolTip(owner, mousePosition)` | The user hovers the icon (`PopupOpen`). Use it for a rich tooltip. | No (`virtual`) |
| `HideToolTip(owner, mousePosition)` | The hover ends (`PopupClose`). | No (`virtual`) |

> [!TIP]
> If you implement `ShowToolTip` / `HideToolTip`, call `icon.SetToolTip(null)` to disable the default tooltip. Showing both can clash.

**Example:** a handler that opens your own flyout window on left click, a small action list on right click, and a custom hover preview.

```csharp
using DotTray;

public sealed class FlyoutHandler : PopupMenuHandler
{
    private readonly MyFlyoutWindow _flyout = new(); // your own UI
    private readonly MyContextWindow _context = new(); // your own UI
    private readonly MyHoverPreview _hover = new(); // your own UI

    public FlyoutHandler(MyFlyoutWindow flyout, MyContextWindow context, MyHoverPreview hover)
    {
        _flyout = flyout;
        _context = context;
        _hover = hover;
    }

    // Left click: toggle the main flyout
    protected override void Show<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition)
    {
        if (_flyout.IsOpen) _flyout.Close();
        else _flyout.OpenAt(mousePosition.X, mousePosition.Y);
    }

    // Right click: show a small context window
    protected override void ShowContext<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition) => _context.OpenAt(mousePosition.X, mousePosition.Y);

    // Optional: rich hover popup instead of the default tooltip
    protected override void ShowToolTip<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition) => _hover.OpenAt(mousePosition.X, mousePosition.Y);

    protected override void HideToolTip<THandler>(NotifyIcon<THandler> owner, MousePosition mousePosition) => _hover.Close();
}
```

Usage:

```csharp
using (var cts = new CancellationTokenSource())
{
    using (var icon = NotifyIcon.Run("app.ico", new FlyoutHandler(), cts.Token))
    {
        icon.SetToolTip(null); // disable the default tooltip because ShowToolTip is overridden
    }
}
```

Things worth knowing when extending:

- `mousePosition` is in **screen coordinates**, ready to position a window at the cursor.
- `owner.NativeWindowHandle` is the window handle behind the icon. It can be used as the owner of child windows or menus. **Use with caution**
- Handler methods are called on the icon's **background STA thread**, the same thread that raises `Interacted`. Marshal to your UI framework's thread if required.

### Which one should I choose?

| I want... | Use |
| --- | --- |
| The built-in native menu with items, checks and submenus | `DefaultPopupMenuHandler` |
| My own popup window or menu on click / right-click / hover | `PopupMenuHandler` |
| Full control over every raw interaction (double clicks, middle clicks, balloon events...) | `INotifyIconHandler` |

---

## Requirements

- **.NET 10** or newer
- **Windows** (the library uses the Win32 shell API and is annotated with `SupportedOSPlatform("windows")`)

<sub>📝 **Documentation notice:** This README was drafted with AI assistance. All content has been read and tested by me. If you spot an error or something unclear, please [open an issue](https://github.com/BlyZeDev/DotTray/issues).</sub>
