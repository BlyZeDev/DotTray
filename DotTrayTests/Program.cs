[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
namespace DotTrayTests;

using System.Threading.Tasks;

sealed class Program
{
    static async Task Main()
    {
        //await DotTrayTest.TestAsync();
        await DotTrayDirectTest.TestAsync();
        
        Console.ReadLine();
    }
}