using System.Linq;
using System.Reflection;

namespace Deipax.Core.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static bool IsStatic(this PropertyInfo source, bool defaultValue = false)
        {
            if (source != null)
            {
                return
                ((source.CanRead && source.GetMethod.IsStatic) ||
                 (source.CanWrite && source.SetMethod.IsStatic));
            }

            return defaultValue;
        }

        public static bool IsPublic(this PropertyInfo source, bool defaultValue = false)
        {
            if (source != null)
            {
                return
                ((source.CanRead && source.GetMethod.IsPublic) ||
                 (source.CanWrite && source.SetMethod.IsPublic));
            }

            return defaultValue;
        }

        public static bool CanRead(this PropertyInfo source, bool defaultValue = false)
        {
            if (source != null)
            {
                return
                    source.CanRead &&
                    source.GetMethod != null;
            }

            return defaultValue;
        }

        public static bool CanWrite(this PropertyInfo source, bool defaultValue = false)
        {
            if (source != null)
            {
                return
                    source.CanWrite &&
                    source.SetMethod != null;
            }

            return defaultValue;
        }

        public static bool HasParameters(this PropertyInfo source, bool defaultValue = false)
        {
            if (source != null)
            {
                return source.GetIndexParameters().Any();
            }

            return defaultValue;
        }
    }
}