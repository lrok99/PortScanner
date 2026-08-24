using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace PortScanner.core
{
    public static class ParseResultExtensions
    {
        public static T Bind<T>(this ParseResult parseResult) where T : class , new() 
        {
            var instance = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                
                if(prop.GetCustomAttribute<CommandAttribute>() is CommandAttribute cmdAttr)
                {
                    //var aliases = new List<string>() { cmdAttr.Name};
                    //if(cmdAttr.Aliases != null && cmdAttr.Aliases.Length > 0)
                    //{
                    //    aliases.AddRange(cmdAttr.Aliases);
                    //}
                    var option = parseResult.RootCommandResult.Children.OfType<OptionResult>().FirstOrDefault(o => cmdAttr.Name == o.Option.Name);
                    if(option != null)
                    {
                        var rawValue = option.GetValueOrDefault<object>();
                        if (rawValue != null)
                        {
                            var targetType = prop.PropertyType;
                            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                            object? convertedValue;
                            if (rawValue.GetType() == underlyingType)
                            {
                                convertedValue = rawValue;

                            }
                            else
                            {
                                convertedValue = Convert.ChangeType(rawValue, underlyingType, CultureInfo.InvariantCulture);
                            }
                            prop.SetValue(instance, convertedValue);
                        }
                    }
                }
            }

            return instance;
        }

        
    }
}
