using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandQuery.Framing
{
    /// <summary>
    /// assembly convention scanner
    /// </summary>
    internal class AssemblyConventionScanner
    {
        private Assembly[] _assemblies;


        private static Lazy<AssemblyConventionScanner> _instance = new Lazy<AssemblyConventionScanner>(() => new AssemblyConventionScanner());
        private Type[] _types;
        private Action<Type> _action;


        /// <summary>
        /// Assemblieses the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns></returns>
        public AssemblyConventionScanner Assemblies(Assembly[] assemblies)
        {
            _assemblies = assemblies;

            return this;
        }

        /// <summary>
        /// Matcheses the specified types.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns></returns>
        public AssemblyConventionScanner Matches(Type[] types)
        {
            _types = types;
            return this;
        }

        /// <summary>
        /// Does the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public AssemblyConventionScanner Do(Action<Type> action)
        {
            _action = action;

            return this;
        }


        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            foreach (var assembly in _assemblies)
            {


                var foundTypes = new List<Type>();


                if (_types != null)
                {
                    foreach (var type in _types)
                    {

                        foundTypes.AddRange(assembly.GetTypes().Where(x =>
                            !IntrospectionExtensions.GetTypeInfo(x).IsAbstract
                            && CanBeCastTo(x, type)));


                    }

                }
                else //scan for all types
                {
                    var badNames = new[] { "System", "Microsoft" };

                    foundTypes.AddRange(assembly.GetTypes().Where(x => !x.GetTypeInfo().IsAbstract && !x.GetTypeInfo().IsInterface && !badNames.Any(b => x.Name.StartsWith(b))));
                }

                foreach (var foundType in foundTypes)
                {
                    _action.Invoke(foundType);
                }
            }
        }

        private static bool CanBeCastTo(Type type, Type destinationType)
        {
            if (type == (Type)null)
                return false;
            if (type == destinationType)
                return true;

            if (destinationType.GetTypeInfo().IsGenericType && !destinationType.GenericTypeArguments.Any())
            {

                if (destinationType.GetTypeInfo().IsInterface && !type.GetTypeInfo().IsInterface)
                {
                    return type.GetInterfaces().Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == destinationType);
                }
            }

            var t1 = destinationType.IsAssignableFrom(type);
            var t2 = type.GetInterfaces().Any(x => x.IsAssignableFrom(destinationType));

            return t1 & t2;
        }
        /// <summary>
        /// Finalizes an instance of the <see cref="AssemblyConventionScanner"/> class.
        /// </summary>
        ~AssemblyConventionScanner()
        {
            _assemblies = null;
            _action = null;
            _types = null;
        }
    }
}