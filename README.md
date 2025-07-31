# DotTray
### Show lightweight Windows Notification Icons simple and easy without any dependencies.

**Current features** - for feature requests just open an issue [here](https://github.com/BlyZeDev/ClipTypr/issues/new?template=enhancement.yml)
- Fully non-blocking API with async support
- Easily create multiple icons at once and handle them individually without any complicated code required
- Changing icon at runtime
- Changing tooltip at runtime
- Changing menu items at runtime
- CancellationToken support to easily tie cancellation to other operations
- Show detailed balloon notifications with customization options
- NativeAOT compatible

## Quickstart
```cs
var menuItems = new MenuItemCollection(
[
    new MenuItem
    {
        Text = "Item Number 1",
        Click = (sender, args) => Console.WriteLine("Item Number 1 Clicked")
    },
    new MenuItem
    {
        Text = "Item Number 2",
        IsDisabled = true
    },
    new MenuItem
    {
        Text = "Item Number 3",
        IsChecked = true,
        Click = (sender, args) => Console.WriteLine($"Item Number 3 was {(sender.IsChecked ?? false ? "checked" : "unchecked")}")
    },
    new MenuItem
    {
        Text = "Item Number 4",
        SubMenu = new MenuItemCollection(
        [
            new MenuItem
            {
                Text = "SubItem Number 1"
            },
            new MenuItem
            {
                Text = "SubItem Number 2",
                Click = (sender, args) => Console.WriteLine("SubItem Number 2 Clicked")
            }
        ])
    }
]);

using (var notifyIcon = NotifyIcon.Run(@"path\to\my\icon.ico", menuItems, CancellationToken.None))
{
    Console.ReadKey();
}
```
This example will look something like this. *The exact look depends on your Windows Version*

<img width="364" height="116" alt="example" src="https://github.com/user-attachments/assets/91b3baa6-d32d-430f-9c25-f3dbbfeda2cc"/>

### Show a Balloon Message
```cs
notifyIcon.ShowBalloon(new BalloonNotification
{
    Icon = BalloonNotificationIcon.Error,
    Title = "An error occurred",
    Message = "This is a fictional error message. You can show some useful information here :)",
    NoSound = false
});
```
This balloon message will look something like this. *The exact look depends on your Windows Version*

<img width="364" height="151" alt="balloonmsg" src="https://github.com/user-attachments/assets/0ada00c4-ea24-4314-9984-a37f1741b62a" />
