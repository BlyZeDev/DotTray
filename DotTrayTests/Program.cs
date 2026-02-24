namespace DotTrayTests;

using DotTray;
using System.Drawing;
using System.Runtime.Versioning;

sealed class Program
{
    [SupportedOSPlatform("windows")]
    static async Task Main()
    {
        var cts = new CancellationTokenSource();

        var tempPath = CreateTestIcon(StockIconId.Error) ?? throw new InvalidOperationException("Icon could not be created");

        var icon = NotifyIcon.Run(tempPath, cts.Token, x =>
        {
            x.BackgroundHoverColor = new TrayColor(218, 83, 225);
            x.BackgroundDisabledColor = new TrayColor(40, 40, 40);
            x.TextDisabledColor = new TrayColor(180, 180, 180);
        }, x => x.LineThickness = 1.2f);
        icon.SetFontSize(16);

        icon.MenuItems.AddItem(x =>
        {
            x.Text = "Sync now";
        });
        icon.MenuItems.AddItem(x =>
        {
            x.Text = $"Next Sync in";
            x.IsDisabled = true;
        });
        icon.MenuItems.AddItem(x =>
        {
            x.Text = "Settings";
            x.SubMenu.AddItem(x =>
            {
                x.Text = "Open Application Folder";
            });
            x.SubMenu.AddItem(x =>
            {
                x.Text = "Autostart";
            });
            x.SubMenu.AddItem(x =>
            {
                x.Text = "Help";
            });
        });
        icon.MenuItems.AddSeparator();
        icon.MenuItems.AddItem(x =>
        {
            x.Text = $"Version 1.0.0";
            x.TextDisabledColor = x.TextColor;
            x.IsDisabled = true;
        });
        icon.MenuItems.AddSeparator();
        icon.MenuItems.AddItem(x =>
        {
            x.Text = "Exit";
            x.Clicked = _ => cts.Cancel();
        });

        try
        {
            File.Delete(tempPath ?? "");
        }
        catch (Exception) { }
    }

    [SupportedOSPlatform("windows")]
    private static string? CreateTestIcon(StockIconId id, StockIconOptions options = StockIconOptions.SmallIcon)
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