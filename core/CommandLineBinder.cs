using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Reflection;
using System.Text;
using System.CommandLine.Parsing;
using System.ComponentModel;

namespace PortScanner.core
{
    public static class CommandLineBinder
    {
        public static RootCommand BuildCommand<TConfig>(string description) where TConfig : class, new()
        {
            var rootCommand = new RootCommand(description);
            var configType = typeof(TConfig);

            foreach (var prop in configType.GetProperties())
            {
                var attr = prop.GetCustomAttribute<CommandAttribute>();
                if (attr == null) continue;

                var propType = prop.PropertyType;                

                // create Option<T>
                var optionType = typeof(Option<>).MakeGenericType(propType);
                // 使用单别名构造（通常是长名 like "--host"）
                var primaryName = attr.Name ?? throw new InvalidOperationException("Option name required");
                var option = (Option)Activator.CreateInstance(optionType, new object[] { primaryName })!;
                
                // 添加短别名（如果存在）
                foreach (var alias in attr.Aliases)
                {
                    option.Aliases.Add(alias);
                }
               

                // 设置描述
                option.Description = attr.Description;

                // 设置默认值工厂（如果有）
                if (attr.DefaultValue != null)
                {
                    object converted;
                    var raw = attr.DefaultValue;
                    // If already of target type, use directly
                    if (raw != null && propType.IsInstanceOfType(raw))
                    {
                        converted = raw;
                    }
                    else
                    {
                        // handle enums
                        if (propType.IsEnum)
                        {
                            converted = Enum.Parse(propType, raw?.ToString() ?? string.Empty);
                        }
                        else
                        {
                            try
                            {
                                converted = Convert.ChangeType(raw!, propType);
                            }
                            catch
                            {
                                var converter = TypeDescriptor.GetConverter(propType);
                                converted = converter.ConvertFrom(raw!);
                            }
                        }
                    }

                    var propInfo = optionType.GetProperty("DefaultValueFactory", BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo != null)
                    {
                        var factoryPropertyType = propInfo.PropertyType;
                        if (factoryPropertyType.IsGenericType && factoryPropertyType.GetGenericTypeDefinition() == typeof(Func<,>))
                        {
                            var firstParam = factoryPropertyType.GetGenericArguments()[0];
                            var factory = CreateDefaultValueFactory(propType, converted, firstParam);
                            propInfo.SetValue(option, factory);
                        }
                    }
                }
                Console.WriteLine($"Added option: {option.Name}, Aliases: {string.Join(", ", option.Aliases)}, Default: {attr.DefaultValue}");

                rootCommand.Add(option);
            }

            return rootCommand;
        }

        // 辅助方法：根据 DefaultValueFactory 所需的参数类型创建对应的委托
        private static Delegate CreateDefaultValueFactory(Type valueType, object value, Type factoryParamType)
        {
            if (factoryParamType == typeof(ParseResult))
            {
                var m = typeof(CommandLineBinder).GetMethod(nameof(CreateDefaultValueFactoryParseResult), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(valueType);
                return (Delegate)m.Invoke(null, new object[] { value })!;
            }
            if (factoryParamType == typeof(ArgumentResult))
            {
                var m = typeof(CommandLineBinder).GetMethod(nameof(CreateDefaultValueFactoryArgumentResult), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(valueType);
                return (Delegate)m.Invoke(null, new object[] { value })!;
            }
            throw new NotSupportedException($"Unsupported DefaultValueFactory parameter type: {factoryParamType}");
        }

        private static Func<ParseResult, T> CreateDefaultValueFactoryParseResult<T>(T value) => _ => value;
        private static Func<ArgumentResult, T> CreateDefaultValueFactoryArgumentResult<T>(T value) => _ => value;
    }
}
