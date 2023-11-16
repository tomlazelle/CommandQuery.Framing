using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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
            new AssemblyConventionScanner()
                .Assemblies(_asm.ToArray())
                .Do(foundInterface =>
                    {
                        var implInterface = foundInterface.GetTypeInfo().ImplementedInterfaces.ToList();
                        implInterface.Add(foundInterface);

                        if (_doNotInclude.Any() && !implInterface.Any(x => _doNotInclude.Any(i => i == x)))
                        {
                            foreach (var type in implInterface)
                            {
                                serviceCollection.AddTransient(type, foundInterface);
                            }

                        }
                    })
                .Execute();
        }
    }
}