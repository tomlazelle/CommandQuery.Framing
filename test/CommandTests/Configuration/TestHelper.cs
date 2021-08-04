using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Shouldly;

namespace CommandTests.Configuration
{
    public static class TestHelper
    {
        public static void PropertyShouldNotBeNull(this object dynamicObject,string propertyName)
        {
            object result = dynamicObject.GetType().GetProperty(propertyName);

            result.ShouldNotBeNull();
        }


        public static string GetTestData(string key)
        {
            var assembly = typeof(TestHelper).GetTypeInfo().Assembly;
            var resourceStream = assembly.GetManifestResourceStream($"PB.SLOrderParse.Tests.TestData.{key}");

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static string[] ToArray(this string data)
        {
            var result = new List<string>();
            var reader = new StringReader(data);
            while (reader.Peek() > 0)
            {
                result.Add(reader.ReadLine());
            }

            return result.ToArray();
        }
    }
}