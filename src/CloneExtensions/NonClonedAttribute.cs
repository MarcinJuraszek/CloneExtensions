using System;

namespace CloneExtensions
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NonClonedAttribute : Attribute
    {
    }
}
