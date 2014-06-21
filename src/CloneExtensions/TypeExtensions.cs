using System;
using System.Linq;

namespace CloneExtensions
{
    static class TypeExtensions
    {
        public static bool IsPrimitiveOrKnownImmutable(this Type type)
        {
            return type.IsPrimitive || CloneFactory.KnownImmutableTypes.Contains(type);
        }
    }
}
