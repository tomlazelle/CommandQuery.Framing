using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Tracks types registered during assembly scanning.
    /// </summary>
    internal class RegistrationInfo
    {
        public List<Type> ImplementationTypes { get; } = new List<Type>();
        public List<(Type ServiceType, Type ImplementationType)> Registrations { get; } = new List<(Type, Type)>();
    }

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
            ScanAndAddTransientTypesWithTracking(serviceCollection, assemblies, types, out _);
            return serviceCollection;
        }

        /// <summary>
        /// Scans and adds transient types, returning tracking information about what was registered.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="types">The types.</param>
        /// <param name="registrationInfo">Output parameter containing registration tracking information.</param>
        public static IServiceCollection ScanAndAddTransientTypesWithTracking(
            this IServiceCollection serviceCollection,
            Assembly[] assemblies,
            Type[] types,
            out RegistrationInfo registrationInfo)
        {
            var info = new RegistrationInfo();
            
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
                    info.ImplementationTypes.Add(implementationType);
                    
                    var serviceTypes = implementationType.GetTypeInfo()
                        .ImplementedInterfaces
                        .ToList();

                    // Optionally add the type itself
                    serviceTypes.Add(implementationType);

                    foreach (var serviceType in serviceTypes.Distinct())
                    {
                        serviceCollection.AddTransient(serviceType, implementationType);
                        info.Registrations.Add((serviceType, implementationType));
                    }
                })
                .Execute();

            registrationInfo = info;
            return serviceCollection;
        }


    }
}