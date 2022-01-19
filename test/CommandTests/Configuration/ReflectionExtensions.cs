using System;

namespace CommandTests.Configuration
{
    public static class ReflectionExtensions
    {
        public static void TryInvoke(this Type type, string method, object instance, object[] paramObjects = null)
        {
            var lifecycleMethod = type.GetMethod(method);

            lifecycleMethod?.Invoke(instance, paramObjects);
        }
    }
}