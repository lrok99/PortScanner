using System;
using System.Threading.Tasks;
using core;
namespace PortScanner
{
    // 简化的 Program：创建并运行 core.Scanner
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("PortScanner start");

            string host = args.Length > 0 ? args[0] : "127.0.0.1";
            int startPort = args.Length > 1 && int.TryParse(args[1], out var sp) ? sp : 1;
            int endPort = args.Length > 2 && int.TryParse(args[2], out var ep) ? ep : 1024;
            int timeout = args.Length > 3 && int.TryParse(args[3], out var to) ? to : 500;

            var scanner = new Scanner(host, startPort, endPort, timeout);
            var spw = new System.Diagnostics.Stopwatch();
            spw.Start();
            await scanner.StartScanningAsync();
            spw.Stop();

            Console.WriteLine("Scanning finished");
            Console.WriteLine($"Scanning time: {spw.ElapsedMilliseconds} ms");
            return 0;
        }
    }
}