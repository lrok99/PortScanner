using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace PortScanner.core
{
    public class ScannerCommand
    {
        private readonly RootCommand _rootCommand;
        private readonly List<Option> _options;

        public ScannerCommand(RootCommand rootCommand, List<Option> options) 
        {
            _rootCommand = rootCommand;
            _options = options;
        }
    }
}
