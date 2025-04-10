using System;
using System.Linq;
using System.Reflection;

namespace CommandQuery.Framing;

/// <summary>
///     assembly convention scanner
/// </summary>
internal class AssemblyConventionScanner
{
    private static readonly Lazy<AssemblyConventionScanner> _instance =
        new(() => new AssemblyConventionScanner());

    private Assembly[] _assemblies;
    private Type[] _targetTypes;
    private Action<Type> _typeAction;

    public static AssemblyConventionScanner Instance => _instance.Value;

    public AssemblyConventionScanner Assemblies(params Assembly[] assemblies)
    {
        _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
        return this;
    }

    public AssemblyConventionScanner Matches(params Type[] types)
    {
        _targetTypes = types;
        return this;
    }

    public AssemblyConventionScanner Do(Action<Type> action)
    {
        _typeAction = action ?? throw new ArgumentNullException(nameof(action));
        return this;
    }

    public void Execute()
    {
        if (_assemblies == null || _typeAction == null)
        {
            throw new InvalidOperationException("Assemblies and action must be set before execution.");
        }

        foreach (var assembly in _assemblies)
        {
            var candidates = assembly.GetTypes().Where(t =>
                !t.GetTypeInfo().IsAbstract &&
                !_IsSystemOrMicrosoftNamespace(t) &&
                (_targetTypes == null || _targetTypes.Any(target => IsAssignableTo(t, target)))
            );

            foreach (var type in candidates)
            {
                _typeAction(type);
            }
        }

        // Optional: Reset internal state after execution
        _assemblies = null;
        _targetTypes = null;
        _typeAction = null;
    }

    private static bool IsAssignableTo(Type type, Type target)
    {
        if (type == null || target == null)
        {
            return false;
        }

        var typeInfo = type.GetTypeInfo();
        var targetInfo = target.GetTypeInfo();

        // Direct assignment check
        if (targetInfo.IsAssignableFrom(typeInfo))
        {
            return true;
        }

        // Open generic check for interfaces and base types
        if (targetInfo.IsGenericTypeDefinition)
        {
            // Check all base types
            while (typeInfo != null && typeInfo.AsType() != typeof(object))
            {
                if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == target)
                {
                    return true;
                }

                typeInfo = typeInfo.BaseType?.GetTypeInfo();
            }

            // Check all interfaces
            return type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == target);
        }

        return false;
    }

    private static bool _IsSystemOrMicrosoftNamespace(Type t)
    {
        var ns = t.Namespace;
        return ns != null &&
               (ns.StartsWith("System", StringComparison.Ordinal) ||
                ns.StartsWith("Microsoft", StringComparison.Ordinal));
    }
}