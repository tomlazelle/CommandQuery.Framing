using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;


namespace CommandQuery.Framing;

/// <summary>
///     assembly convention scanner
/// </summary>

internal class AssemblyConventionScanner
{
    private Assembly[] _assemblies;
    private Type[] _types;
    private Action<Type> _action;
    private readonly ILogger<AssemblyConventionScanner> _logger;

    public AssemblyConventionScanner(ILogger<AssemblyConventionScanner> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public AssemblyConventionScanner Assemblies(params Assembly[] assemblies)
    {
        _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
        return this;
    }

    public AssemblyConventionScanner Matches(params Type[] types)
    {
        _types = types;
        return this;
    }

    public AssemblyConventionScanner Do(Action<Type> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        return this;
    }

    public void Execute()
    {
        foreach (var assembly in _assemblies)
        {
            Type[] assemblyTypes;

            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                assemblyTypes = ex.Types.Where(t => t != null).ToArray();

                _logger.LogWarning("ReflectionTypeLoadException occurred for assembly {AssemblyName}. Partial types loaded.",
                    assembly.FullName);

                foreach (var loaderException in ex.LoaderExceptions)
                {
                    _logger.LogWarning(loaderException, "Loader exception while scanning assembly {AssemblyName}", assembly.FullName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while loading types from assembly {AssemblyName}", assembly.FullName);
                continue;
            }

            var foundTypes = new List<Type>();

            if (_types != null)
            {
                foreach (var targetType in _types)
                {
                    foundTypes.AddRange(assemblyTypes.Where(x =>
                        !x.GetTypeInfo().IsAbstract &&
                        CanBeCastTo(x, targetType)));
                }
            }
            else
            {
                var badPrefixes = new[] { "System", "Microsoft" };

                foundTypes.AddRange(assemblyTypes.Where(x =>
                    !x.GetTypeInfo().IsAbstract &&
                    !x.GetTypeInfo().IsInterface &&
                    !string.IsNullOrWhiteSpace(x.Namespace) &&
                    !badPrefixes.Any(b => x.Namespace.StartsWith(b))));
            }

            foreach (var foundType in foundTypes.Distinct())
            {
                try
                {
                    _action?.Invoke(foundType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error invoking action for type {TypeName}", foundType.FullName);
                }
            }
        }
    }

    private static bool CanBeCastTo(Type type, Type destinationType)
    {
        if (type == null || destinationType == null)
            return false;

        if (type == destinationType)
            return true;

        var destInfo = destinationType.GetTypeInfo();
        var typeInfo = type.GetTypeInfo();

        if (destInfo.IsGenericType && !destInfo.GenericTypeArguments.Any())
        {
            if (destInfo.IsInterface && !typeInfo.IsInterface)
            {
                return type.GetInterfaces().Any(x =>
                    x.GetTypeInfo().IsGenericType &&
                    x.GetGenericTypeDefinition() == destinationType);
            }
        }

        return destinationType.IsAssignableFrom(type) ||
               type.GetInterfaces().Any(x => x.IsAssignableFrom(destinationType));
    }
}
