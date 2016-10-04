using System;
using System.Linq;
using System.Reflection;

namespace CloneExtensions
{
    static class TypeExtensions
    {
        public static bool IsPrimitiveOrKnownImmutable(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive || CloneFactory.KnownImmutableTypes.Contains(type);
        }
    }
}
