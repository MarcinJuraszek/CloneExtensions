using System.Reflection;

namespace Deipax.Core.Extensions
{
    public static class FieldInfoExtensions
    {
        public static bool IsBackingField(this FieldInfo source, bool defaultValue = false)
        {
            if (source != null)
            {
                return source.Name.IndexOf(">k__BackingField", 0) >= 0;
            }

            return defaultValue;
        }
    }
}