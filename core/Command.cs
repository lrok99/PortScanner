using System;
using System.CommandLine;
namespace core;
public class Command
{
	private readonly RootCommand _rootCommand;
	public Command()
	{
        _rootCommand = new ("This is a simple CLI application.");
        ConfigureCommandLine();
    }

	private void ConfigureCommandLine()
    {
        _rootCommand.AddOption(new Option<string>("--host")
        {
            Description = "The host to scan (default: 127.0.0.1)",
            DefaultValueFactory = parseResult => "127.0.0.1"
        });
        _rootCommand.SetAction(parseResult => 
        {
           var host = parseResult.GetValueForOption<string>("--host");
        });
    }


    private void ConfigureHost() { 
        Option<string> hostOption = new Option<string>("--host")
        {
            Description = "The host to scan (default: 127.0.0.1)",
            DefaultValueFactory = parseResult => "127.0.0.1"
        };
        _rootCommand.AddOption(hostOption);
        _rootCommand.
            SetAction(parseResult =>
        {
            var host = parseResult.GetValue(hostOption);
            Console.WriteLine($"Host: {host}");
        });
    }
}
