using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace PortScanner.core
{
    public static class ScannerCommandBuilderExtensions
    {
        private const string DefaultHost = "127.0.0.1";
        public static ScannerCommandBuilder AddTargetOption(this ScannerCommandBuilder builder)
        {
            var hostOption = new Option<string>("--host") 
            {
                Aliases = { "-h" },
                Description = "The host to scan (IP address or hostname).",
                DefaultValueFactory = parseResult => DefaultHost,
            };

            var startPortOption = new Option<int>("--start-port")
            {
                Aliases = { "-sp" },
                Description = "The starting port number for the scan.",
                DefaultValueFactory = parseResult => 1,
                CustomParser = parseResult =>
                {
                    var port = parseResult.GetValueOrDefault<int>();
                    if (port < 1 || port > 65535)
                    {
                        parseResult.AddError("Port number is out of range.");
                    }
                    return port;
                },
            };

            var endPortOption = new Option<int>("--end-port")
            {
                Aliases = { "-ep" },
                Description = "The ending port number for the scan.",
                DefaultValueFactory = parseResult => 65535,
                CustomParser = parseResult =>
                {
                    var port = parseResult.GetValueOrDefault<int>();
                    if (port < 1 || port > 65535)
                    {
                        parseResult.AddError("Port number is out of range.");
                    }
                    return port;
                },
            };
            

            return builder.AddOption(hostOption).AddOption(startPortOption).AddOption(endPortOption);
        }

        public static ScannerCommandBuilder AddTExecutionOption(this ScannerCommandBuilder builder)
        {
            var timeoutOption = new Option<int>("--timeout")
            {
                Aliases = { "-t" },
                Description = "The timeout in milliseconds for each port scan.",
                DefaultValueFactory = parseResult => 500,
                CustomParser = parseResult =>
                {
                    var timeout = parseResult.GetValueOrDefault<int>();
                    if (timeout < 1)
                    {
                        parseResult.AddError("Timeout must be a positive integer.");
                    }
                    return timeout;
                },
            };

            var concurrencyOption = new Option<int>("--concurrency")
            {
                Aliases = { "-c" },
                Description = "The number of concurrent scans to perform.",
                DefaultValueFactory = parseResult => 100,
                CustomParser = parseResult =>
                {
                    var concurrency = parseResult.GetValueOrDefault<int>();
                    if (concurrency < 1 || concurrency > 1000)
                    {
                        parseResult.AddError("Concurrency must be a positive integer between 1 and 1000.");
                    }
                    return concurrency;
                },
            };
            return builder.AddOption(timeoutOption).AddOption(concurrencyOption);
        }

        public static ScannerCommandBuilder AddOutputOption(this ScannerCommandBuilder builder)
        {
            var outputOption = new Option<string>("--output")
            {
                Aliases = { "-o" },
                Description = "The output file to save the scan results.",
                DefaultValueFactory = parseResult => Environment.CurrentDirectory,
            };
            return builder.AddOption(outputOption);
        }

    }
}
