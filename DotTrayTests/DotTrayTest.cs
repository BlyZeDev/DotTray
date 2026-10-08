namespace DotTrayTests;

using DotTray;
using DotTray.Items;
using System;
using System.Drawing;

internal class DotTrayTest
{
    public static async Task TestAsync()
    {
        using (var cts = new CancellationTokenSource())
        {
            using (var tempIcon = CreateTestIcon(StockIconId.Application))
            {
                using (var handler = new DefaultPopupMenuHandler())
                {
                    using (var icon = await NotifyIcon.RunAsync(tempIcon.FilePath, handler, cts.Token))
                    {
                        handler.Items.Add(new MenuItem
                        {
                            Text = "Standard Action",
                            Clicked = item => Console.WriteLine("Action executed!")
                        });

                        handler.Items.Add(new CheckItem
                        {
                            Text = "Enable Background Sync",
                            IsChecked = true,
                            Clicked = item => Console.WriteLine($"Sync is now {(item.IsChecked ? "ON" : "OFF")}")
                        });

                        handler.Items.Add(SeparatorItem.Instance);

                        var submenu = new SubmenuItem { Text = "Advanced Settings" };

                        submenu.Items.Add(new MenuItem
                        {
                            Text = "Dynamic Item (Click to update time)",
                            Clicked = item => item.Text = $"Last clicked: {DateTime.Now:HH:mm:ss}"
                        });

                        submenu.Items.Add(new MenuItem
                        {
                            Text = "Premium Feature (Locked)",
                            IsDisabled = true
                        });

                        handler.Items.Add(submenu);

                        handler.Items.Add(SeparatorItem.Instance);

                        handler.Items.Add(new MenuItem
                        {
                            Text = "Exit Application",
                            Clicked = _ =>
                            {
                                Console.WriteLine("Exiting gracefully via tray menu...");
                                cts.Cancel();
                            }
                        });

                        Console.WriteLine("Win32 icon is running");

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