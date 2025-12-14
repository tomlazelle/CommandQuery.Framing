using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandQuery.Framing
{

    internal class Scanner
    {
        private readonly List<Assembly> _asm = new List<Assembly>();
        private readonly List<Type> _doNotInclude = new List<Type>();

        public Scanner ScanAssemblyWithType<T>()
        {
            _asm.Add(typeof(T).Assembly);

            return this;
        }

        public Scanner DoNotIncludeType<T>()
        {
            _doNotInclude.Add(typeof(T));

            return this;
        }
        public void Scan(IServiceCollection serviceCollection)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var logger = loggerFactory.CreateLogger<AssemblyConventionScanner>();

            new AssemblyConventionScanner(logger)
                .Assemblies(_asm.ToArray())
                .Do(foundInterface =>
                    {
                        var implInterface = foundInterface.GetTypeInfo().ImplementedInterfaces.ToList();
                        implInterface.Add(foundInterface);

                        // Skip registration if any implemented interface is in the exclusion list
                        bool shouldExclude = implInterface.Any(iface => _doNotInclude.Contains(iface));
                        if (shouldExclude)
                        {
                            return;
                        }

                        // Register all interfaces implemented by this type
                        foreach (var type in implInterface)
                        {
                            serviceCollection.AddTransient(type, foundInterface);
                        }
                    })
                .Execute();
        }
    }
}