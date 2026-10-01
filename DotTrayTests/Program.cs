[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
namespace DotTrayTests;

using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

sealed class Program
{
    static async Task Main()
    {
        using (var cts = new CancellationTokenSource())
        {
            using (var tempIcon = CreateTestIcon(StockIconId.DesktopPC))
            {
                var win32Icon = await Win32Icon.RunAsync(tempIcon.FilePath, cts);
                var drawingIcon = await DrawingIcon.RunAsync(tempIcon.FilePath, cts);

                try
                {
                    await Task.Delay(Timeout.Infinite, cts.Token);
                }
                catch (OperationCanceledException) { }

                win32Icon.Dispose();
                drawingIcon.Dispose();
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