[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace DotTrayTests;

using DotTray;
using System.Drawing;
using System.Threading.Tasks;

sealed class Program
{
    static async Task Main()
    {
        var cts = new CancellationTokenSource();

        using var icon = await NotifyIcon.RunAsync(CreateTestIcon(StockIconId.DeviceCamera), cts.Token);
        icon.SetToolTip(cts.ToString());

        Console.ReadLine();
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