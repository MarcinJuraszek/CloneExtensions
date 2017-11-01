using System.Reflection;

namespace CloneExtensions.Extensions
{
    public static class MethodInfoExtensions
    {
        public static bool HasAttribute(
            this MethodInfo source,
            MethodAttributes attr,
            bool defaultValue = false)
        {
            if (source != null)
            {
                return (source.Attributes & attr) == attr;
            }

            return defaultValue;
        }
    }
}