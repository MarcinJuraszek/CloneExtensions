using System;
using System.Reflection;

namespace CloneExtensions.Interfaces
{
    public interface IModelInfo
    {
        MemberInfo MemberInfo { get; }
        string Name { get; }
        Type Type { get; }
        bool IsStatic { get; }
        bool IsPublic { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        int Depth { get; }
        bool IsLiteral { get; }
    }

    public interface IFieldModelInfo : IModelInfo
    {
        FieldInfo FieldInfo { get; }
        bool IsBackingField { get; }
    }

    public interface IPropertyModelInfo : IModelInfo
    {
        IFieldModelInfo BackingField { get; }
        PropertyInfo PropertyInfo { get; }
        bool HasParameters { get; }
        bool HasBackingField { get; }
        bool IsAbstract { get; }
        bool IsVirtual { get; }
        bool IsNew { get; }
    }
}