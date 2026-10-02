namespace DotTrayTests;

using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

sealed class Program
{
    static async Task Main()
    {
        using (var cts = new CancellationTokenSource())
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                using (var tempIcon = Win32Icon.CreateTestIcon(StockIconId.DesktopPC))
                {
                    using (var win32Icon = await Win32Icon.RunAsync(tempIcon.FilePath, cts))
                    {
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
}