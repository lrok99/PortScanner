using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using System.Threading;

namespace PortScanner.core
{
    public class ScannerCommandBuilder
    {
        private readonly RootCommand _rootCommand;
        private readonly List<Option> _options = new List<Option>();

        public ScannerCommandBuilder(string description = "This is a simple Port Scanner CLI application.")
        {
            _rootCommand = new RootCommand(description);
        }

        public ScannerCommandBuilder AddOption(Option option)
        {
            if (option is null) throw new ArgumentNullException(nameof(option));
            _options.Add(option);
            _rootCommand.Add(option);
            return this;
        }

        public ScannerCommand Build()
        {
            var scannerCommand = new ScannerCommand(_rootCommand, _options);
            // Use the overload that provides a CancellationToken to the handler
            scannerCommand.RootCommand.SetAction(async (parseResult, token) => await scannerCommand.ExecuteAsync(parseResult, token));
            return scannerCommand;
        }
    }
}
