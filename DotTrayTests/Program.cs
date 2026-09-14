[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace DotTrayTests;

using DotTray;
using DotTray.Default;
using System.Drawing;
using System.Threading.Tasks;

sealed class Program
{
    static async Task Main()
    {
        var cts = new CancellationTokenSource();

        using var icon = await NotifyIcon.RunAsync(CreateTestIcon(StockIconId.DeviceCamera), new DefaultPopupMenuHandler(), cts.Token);
        icon.SetToolTip(cts.ToString());

        icon.Handler.Items.Add(new MenuItem
        {
            Text = "Test",
            Clicked = x => Console.WriteLine($"{x.Text} clicked")
        });
        icon.Handler.Items.Add(SeparatorItem.Instance);
        icon.Handler.Items.Add(new SubmenuItem
        {
            Text = "Test 2",
            Clicked = x => Console.WriteLine($"{x.Text} clicked"),
        });

        var item = icon.Handler.Items[2] as SubmenuItem;
        item?.Items.Add(new MenuItem
        {
            Text = "This is a submenu item",
            IsDisabled = true,
        });
        item?.Items.Add(SeparatorItem.Instance);

        await Task.Delay(5000);
        Console.WriteLine("Update");

        (icon.Handler.Items[2] as MenuItem)?.Text += " Edit!";
        icon.Handler.Items.Add(new MenuItem
        {
            Text = "New Item"
        });
        item?.Items.Add(new CheckItem
        {
            Text = "This is also a submenu item",
            IsDisabled = false,
            IsChecked = true,
            Clicked = x => Console.WriteLine($"{x.Text} check state is: {x.IsChecked}")
        });

        Console.ReadLine();
    }

    private static string? CreateTestIcon(StockIconId id, StockIconOptions options = StockIconOptions.ShellIconSize)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"temptesticon.ico");

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