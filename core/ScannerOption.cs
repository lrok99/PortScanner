using System;
using System.Collections.Generic;
using System.Text;
namespace PortScanner.core
{
    public class ScannerOption
    {
        public required string Host { get; set; }
     
        public required int StartPort { get; set; }

        
        public required int EndPort { get; set; }

     
        public int Timeout { get; set; } = 500; // Default timeout in milliseconds
       
        public int Concurrency { get; set; } = 200; // Default number of concurrent tasks

     
        public string Output { get; set; } = string.Empty; // Default output format

    }
}
