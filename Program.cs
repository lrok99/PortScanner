using System;
using System.Threading.Tasks;
using PortScanner.core;
namespace PortScanner
{
    // 简化的 Program：创建并运行 core.Scanner
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var command = new ScannerCommandBuilder().AddTargetOption().AddTExecutionOption().AddOutputOption().Build();

            var parseResult = command.RootCommand.Parse(args);
            if(parseResult.Errors.Count > 0)
            {
                foreach (var error in parseResult.Errors)
                {
                    Console.WriteLine(error.Message);
                }
                return -1;
            }

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };
            return await parseResult.InvokeAsync(cancellationToken:cts.Token);
            
        }
    }
}