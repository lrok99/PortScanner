using System;
using System.Collections.Generic;
using System.Text;

namespace PortScanner.core
{
    [AttributeUsage(AttributeTargets.Property,Inherited = false,AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public required string Name { get; set; }
        public string[] Aliases { get; set; } = Array.Empty<string>();
        public string? Description { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = false;

        public object? DefaultValue { get; set; } = null;
    }
}
