using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{
    /// <summary>
    /// type extensions
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Scans the and add transient types.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="types">The types.</param>
        internal static IServiceCollection ScanAndAddTransientTypes(this IServiceCollection serviceCollection, Assembly[] assemblies, Type[] types)
        {
            new AssemblyConventionScanner()
                .Assemblies(assemblies)
                .Matches(types)
                .Do(foundInterface =>
                {
                    var implInterface = foundInterface.GetTypeInfo().ImplementedInterfaces.ToList();
                    implInterface.Add(foundInterface);

                    foreach (var type in implInterface)
                    {
                        serviceCollection.AddTransient(type, foundInterface);
                    }

                }).Execute();

            return serviceCollection;
        }

    }
}