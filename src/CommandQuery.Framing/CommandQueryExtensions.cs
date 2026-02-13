using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Extension methods for configuring CommandQuery services in dependency injection.
    /// </summary>
    public static class CommandQueryExtensions
    {
        /// <summary>
        /// Adds CommandQuery framework services to the dependency injection container.
        /// Registers the broker, domain event publisher, and scans assemblies for handlers and domain events.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add services to.</param>
        /// <param name="handlers">Assemblies to scan for handlers (IHandler, IAsyncHandler) and domain events (IDomainEvent).</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddCommandQuery(typeof(Startup).Assembly);
        /// </code>
        /// </example>
        public static IServiceCollection AddCommandQuery(this IServiceCollection serviceCollection, params Assembly[] handlers)
        {

            serviceCollection.AddSingleton<IBroker, Broker>();
            serviceCollection.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();

            serviceCollection.ScanAndAddTransientTypes(handlers,
                                                       new Type[]
                                                       {
                                                           typeof(IAsyncHandler<,>),
                                                           typeof(IHandler<,>),
                                                           typeof(IDomainEvent<>)
                                                       });

            return serviceCollection;
        }

        /// <summary>
        /// Adds CommandQuery framework services to the dependency injection container with optional configuration.
        /// Registers the broker, domain event publisher, and scans assemblies for handlers and domain events.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add services to.</param>
        /// <param name="options">Configuration options for registration behavior.</param>
        /// <param name="handlers">Assemblies to scan for handlers (IHandler, IAsyncHandler) and domain events (IDomainEvent).</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddCommandQuery(new CommandQueryOptions 
        /// { 
        ///     LogRegistrations = true, 
        ///     ValidateRegistrations = true 
        /// }, typeof(Startup).Assembly);
        /// </code>
        /// </example>
        public static IServiceCollection AddCommandQuery(
            this IServiceCollection serviceCollection, 
            CommandQueryOptions options,
            params Assembly[] handlers)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            serviceCollection.AddSingleton<IBroker, Broker>();
            serviceCollection.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();

            var targetTypes = new Type[]
            {
                typeof(IAsyncHandler<,>),
                typeof(IHandler<,>),
                typeof(IDomainEvent<>)
            };

            serviceCollection.ScanAndAddTransientTypesWithTracking(
                handlers,
                targetTypes,
                out var registrationInfo);

            // Feature 1: Log all registered handlers and domain events
            if (options.LogRegistrations)
            {
                LogRegistrations(registrationInfo, targetTypes);
            }

            // Feature 2: Validate all handlers in the assemblies were registered
            if (options.ValidateRegistrations)
            {
                ValidateRegistrations(handlers, registrationInfo, targetTypes);
            }

            return serviceCollection;
        }

        private static void LogRegistrations(RegistrationInfo registrationInfo, Type[] targetTypes)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var logger = loggerFactory.CreateLogger("CommandQuery.Framing.Registration");

            logger.LogInformation("=== CommandQuery Registration Summary ===");
            logger.LogInformation("Total implementation types found: {Count}", registrationInfo.ImplementationTypes.Count);

            // Group by target type (IAsyncHandler, IHandler, IDomainEvent)
            foreach (var targetType in targetTypes)
            {
                var matchingTypes = registrationInfo.ImplementationTypes
                    .Where(t => t.GetInterfaces()
                        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetType))
                    .ToList();

                if (matchingTypes.Any())
                {
                    var typeName = targetType.Name;
                    logger.LogInformation("--- {TypeName} ({Count}) ---", typeName, matchingTypes.Count);
                    
                    foreach (var implType in matchingTypes.OrderBy(t => t.FullName))
                    {
                        logger.LogInformation("  → {TypeName}", implType.FullName);
                    }
                }
            }

            logger.LogInformation("=== End Registration Summary ===");
        }

        private static void ValidateRegistrations(
            Assembly[] assemblies, 
            RegistrationInfo registrationInfo,
            Type[] targetTypes)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });

            var logger = loggerFactory.CreateLogger("CommandQuery.Framing.Validation");

            // Find all types in the assemblies that implement the target interfaces
            var allHandlerTypes = assemblies
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null);
                    }
                })
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => targetTypes.Any(targetType =>
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == targetType)))
                .ToList();

            var registeredTypes = registrationInfo.ImplementationTypes.ToHashSet();
            var unregisteredTypes = allHandlerTypes.Where(t => !registeredTypes.Contains(t)).ToList();

            if (unregisteredTypes.Any())
            {
                logger.LogError("=== Validation Failed: Unregistered Handlers Found ===");
                foreach (var type in unregisteredTypes)
                {
                    logger.LogError("  ✗ {TypeName} was not registered", type.FullName);
                }
                logger.LogError("=== End Validation ===");

                throw new InvalidOperationException(
                    $"Validation failed: {unregisteredTypes.Count} handler(s) were not registered. " +
                    $"Types: {string.Join(", ", unregisteredTypes.Select(t => t.Name))}");
            }
            else
            {
                logger.LogInformation("=== Validation Passed ===");
                logger.LogInformation("All {Count} handler types in the assemblies were successfully registered.", allHandlerTypes.Count);
                logger.LogInformation("=== End Validation ===");
            }
        }

    }
}