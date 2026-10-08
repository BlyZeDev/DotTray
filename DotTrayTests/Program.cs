[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
namespace DotTrayTests;

using DotTray.Direct;
using System.Threading.Tasks;

sealed class Program
{
    static async Task Main()
    {
        //await DotTrayTest.TestAsync();
        var menu = new PopupMenu(nint.Zero);

        Console.ReadLine();
    }
}