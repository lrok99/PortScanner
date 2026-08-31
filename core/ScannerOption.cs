using System;
using System.ComponentModel;

namespace PortScanner.core
{
    public sealed record ScannerOption
    {
        public const int MinPort = 1;
        public const int MaxPort = 65535;
        [Command(Name = "--host", IsRequired = true, Description = "The host to scan", DefaultValue = "127.0.0.1")]
        public string Host { get; init; }
        [Command(Name = "--start-port", Aliases = new[] { "-s" }, Description = "The starting port to scan", DefaultValue = 1)]
        public int StartPort { get; init; }
        [Command(Name = "--end-port", Aliases = new[] { "-e" }, Description = "The ending port to scan", DefaultValue = 1000)]
        public int EndPort { get; init; }
        [Command(Name = "--timeout", Aliases = new[] { "-t" }, Description = "The timeout in milliseconds", DefaultValue = 500)]
        public int Timeout { get; init; }
        [Command(Name = "--concurrency", Aliases = new[] { "-c" }, Description = "The number of concurrent tasks", DefaultValue = 200)]
        public int Concurrency { get; init; }
        [Command(Name = "--output", Aliases = new[] { "-o" }, Description = "The output file path")]
        public string? Output { get; init; }

    }
}
