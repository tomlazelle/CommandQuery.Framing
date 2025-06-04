using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;

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
        public static IServiceCollection ScanAndAddTransientTypes(
            this IServiceCollection serviceCollection,
            Assembly[] assemblies,
            Type[] types)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var logger = loggerFactory.CreateLogger<AssemblyConventionScanner>();

            new AssemblyConventionScanner(logger)
                .Assemblies(assemblies)
                .Matches(types)
                .Do(implementationType =>
                {
                    var serviceTypes = implementationType.GetTypeInfo()
                        .ImplementedInterfaces
                        .ToList();

                    // Optionally add the type itself
                    serviceTypes.Add(implementationType);

                    foreach (var serviceType in serviceTypes.Distinct())
                    {
                        serviceCollection.AddTransient(serviceType, implementationType);
                    }
                })
                .Execute();

            return serviceCollection;
        }

    }
}