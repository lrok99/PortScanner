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

        //public ScannerOption(string host, int startPort, int endPort, int timeout = 500, int concurrency = 200, string? output = null)
        //{
        //    if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("host cannot be null or empty", nameof(host));
        //    if (startPort < MinPort || startPort > MaxPort) throw new ArgumentOutOfRangeException(nameof(startPort));
        //    if (endPort < MinPort || endPort > MaxPort) throw new ArgumentOutOfRangeException(nameof(endPort));
        //    if (startPort > endPort) throw new ArgumentException("startPort must be less than or equal to endPort");
        //    if (timeout < 1) throw new ArgumentOutOfRangeException(nameof(timeout));
        //    if (concurrency < 1) throw new ArgumentOutOfRangeException(nameof(concurrency));

        //    Host = host;
        //    StartPort = startPort;
        //    EndPort = endPort;
        //    Timeout = timeout;
        //    Concurrency = concurrency;
        //    Output = output;
        //}

        //public ScannerOption()
        //{

        //}
    }
}
