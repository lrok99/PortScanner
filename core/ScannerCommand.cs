
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine.Parsing;

namespace PortScanner.core
{
    public class ScannerCommand
    {
        private readonly RootCommand _rootCommand;
        private readonly List<Option> _options;
        private readonly Dictionary<string, Option> _aliasMap;

        public ScannerCommand(RootCommand rootCommand, List<Option> options) 
        {
            _rootCommand = rootCommand;
            _options = options;
            _aliasMap = _options
                        .Where(e => e.Aliases != null && e.Aliases.Any())
                        .SelectMany(o => new[] { o.Name }.Concat(o.Aliases ?? Enumerable.Empty<string>()), (o, alias) => new { Alias = alias, Option = o })
                        .ToDictionary(x => x.Alias, x => x.Option);
        }

        public RootCommand RootCommand => _rootCommand;

        public async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            var host = GetValue<string>(parseResult, "--host");
            var startPort = GetValue<int>(parseResult, "--start-port");
            var endPort = GetValue<int>(parseResult, "--end-port");
            var timeout = GetValue<int>(parseResult, "--timeout");
            var concurrency = GetValue<int>(parseResult, "--concurrency");
            var output = GetValue<string>(parseResult, "--output");
            
            if(startPort > endPort)
            {
                Console.WriteLine("Error: Start port cannot be greater than end port.");
                return 1;
            }
            var resolvedHost = await ResolveHostAsync(host);
            if (resolvedHost == null) 
            { 
                Console.WriteLine($"Error: Unable to resolve host '{host}' to an IP address."); 
                return 1;
            }
            try
            {
                var scanner = new Scanner(host, startPort, endPort, timeout, concurrency);
                await scanner.StartScanningAsync(cancellationToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
            return 0;
        }

        private T GetValue<T>(ParseResult parseResult, string alias)
        {
            if(parseResult is null) throw new ArgumentNullException(nameof(parseResult));
            if(string.IsNullOrEmpty(alias)) return default!;
            if(!_aliasMap.TryGetValue(alias, out var option)) return default!;
            var result = parseResult.GetResult(option);
            return result is null ? default! : result.GetValueOrDefault<T>();
        }

        private async Task<IPAddress> ResolveHostAsync(string host)
        {
            try
            {
                if(IPAddress.TryParse(host, out var result))
                {
                    return result;
                }
                var hostAddress = await Dns.GetHostAddressesAsync(host);
                
                return hostAddress.FirstOrDefault(e=>e.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)??throw new Exception($"No valid IP address found for host: {host}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to resolve host '{host}': {ex.Message}");
            }
        }
    }
}
