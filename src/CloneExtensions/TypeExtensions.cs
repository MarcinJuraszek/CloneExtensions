using CloneExtensions.Extensions;
using CloneExtensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CloneExtensions
{
    static class TypeExtensions
    {
        public static bool IsPrimitiveOrKnownImmutable(this Type type)
        {
            return type.IsPrimitive() || CloneFactory.KnownImmutableTypes.Contains(type);
        }

        public static bool UsePrimitive(this Type type)
        {
            return type.IsPrimitiveOrKnownImmutable() || typeof(Delegate).IsAssignableFrom(type);
        }

#if NET40 || NET45 || NET461
        public static bool IsAbstract(this Type type)
        {
            return type.IsAbstract;
        }
        
        public static bool IsPrimitive(this Type type)
        {
            return type.IsPrimitive;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsInterface(this Type type)
        {
            return type.IsInterface;
        }
#else

        public static ConstructorInfo[] GetConstructors(this Type type)
        {
            return type.GetTypeInfo().GetConstructors();
        }

        public static ConstructorInfo GetConstructor(this Type type, params Type[] types)
        {
            return type.GetTypeInfo().GetConstructor(types);
        }

        public static Type[] GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().GetInterfaces();
        }

        public static bool IsAssignableFrom(this Type type, Type c)
        {
            return type.GetTypeInfo().IsAssignableFrom(c);
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition
                ? type.GetTypeInfo().GenericTypeParameters
                : type.GetTypeInfo().GenericTypeArguments;
        }

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }
#endif

        public static IReadOnlyList<IFieldModelInfo> GetAllFields(this Type type)
        {
            return GetAllFieldsHelper(type, 0);
        }

        public static IReadOnlyList<IPropertyModelInfo> GetAllProperties(
            this Type type,
            IEnumerable<IFieldModelInfo> fields = null)
        {
            var allFields = fields != null ?
                fields :
                type.GetAllFields();

            return GetAllPropertiesHelper(allFields, type, 0);
        }

        public static IReadOnlyList<IPropertyModelInfo> GetFilteredProperties(
            this Type type,
            IEnumerable<IFieldModelInfo> fields = null)
        {
            List<IPropertyModelInfo> properties = new List<IPropertyModelInfo>();
            var allProperties = type.GetAllProperties(fields);

            // If properties that share a name are marked as abstract
            // or virtual, then only one of them is needed in order to
            // set/get the value of the property.
            allProperties
                .Where(x => x.IsAbstract || x.IsVirtual)
                .Select(x => x)
                .GroupBy(x => x.Name)
                .ToList()
                .ForEach(x =>
                {
                    // All Abstract properties are "new"
                    // All initial implemention of virtual properties 
                    // are "new".  Foreach "new" property
                    // find the property at the lowest depth.
                    foreach (var item in x.Where(y => y.IsNew))
                    {
                        var itemToAdd = item;

                        foreach (var p in x.Where(y => y.Depth < item.Depth).OrderByDescending(y => y.Depth))
                        {
                            if (!p.IsNew)
                            {
                                itemToAdd = p;
                            }
                            else
                            {
                                break;
                            }
                        }

                        properties.Add(itemToAdd);
                    }
                });

            // Add all non-virtual/non-abstract properties
            properties.AddRange(allProperties.Where(x => !(x.IsAbstract || x.IsVirtual)).Select(x => x));

            return properties;
        }

        #region Private Members
        private static IReadOnlyList<IFieldModelInfo> GetAllFieldsHelper(
            Type type,
            int depth = 0)
        {
            if (type == null)
            {
                return new List<IFieldModelInfo>();
            }

            var fields = type
                .GetTypeInfo()
                .GetFields(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly)
                .Select(x => FieldModelInfo.Create(x, depth))
                .ToList();

            fields.AddRange(GetAllFieldsHelper(type.GetTypeInfo().BaseType, ++depth));

            return fields;
        }

        private static IReadOnlyList<IPropertyModelInfo> GetAllPropertiesHelper(
            IEnumerable<IFieldModelInfo> fields,
            Type type,
            int depth = 0)
        {
            if (type == null)
            {
                return new List<IPropertyModelInfo>();
            }

            var props = type
                .GetTypeInfo()
                .GetProperties(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly)
                .Select(x => PropertyModelInfo.Create(fields, x, depth))
                .ToList();

            props.AddRange(GetAllPropertiesHelper(fields, type.GetTypeInfo().BaseType, ++depth));

            return props;
        }
        #endregion

        #region Helpers
        [DebuggerDisplay("{FieldInfo.FieldType.Name} {FieldInfo.Name} {FieldInfo.DeclaringType.Name}")]
        class FieldModelInfo : IFieldModelInfo
        {
            public static IFieldModelInfo Create(
                FieldInfo info,
                int depth)
            {
                return new FieldModelInfo()
                {
                    Name = info.Name,
                    Type = info.FieldType,
                    FieldInfo = info,
                    MemberInfo = info,
                    IsStatic = info.IsStatic,
                    IsPublic = info.IsPublic,
                    CanRead = true,
                    CanWrite = !info.IsInitOnly && !info.IsLiteral,
                    IsBackingField = info.IsBackingField(false),
                    Depth = depth,
                    IsLiteral = info.IsLiteral
                };
            }

            public string Name { get; private set; }
            public Type Type { get; private set; }
            public MemberInfo MemberInfo { get; private set; }
            public FieldInfo FieldInfo { get; private set; }
            public bool IsStatic { get; private set; }
            public bool IsPublic { get; private set; }
            public bool CanRead { get; private set; }
            public bool CanWrite { get; private set; }
            public bool IsBackingField { get; private set; }
            public int Depth { get; private set; }
            public bool IsLiteral { get; private set; }
        }

        [DebuggerDisplay("{PropertyInfo.PropertyType.Name} {PropertyInfo.Name} {PropertyInfo.DeclaringType.Name}")]
        class PropertyModelInfo : IPropertyModelInfo
        {
            public static IPropertyModelInfo Create(
                IEnumerable<IFieldModelInfo> fields,
                PropertyInfo info,
                int depth)
            {
                if (fields == null) throw new ArgumentNullException(nameof(fields));
                string key = string.Format("<{0}>k__BackingField", info.Name);

                var backingField = fields
                    .Where(x =>
                        x.IsBackingField &&
                        string.Equals(x.FieldInfo.Name, key) &&
                        x.FieldInfo.DeclaringType == info.DeclaringType &&
                        x.Depth == depth)
                    .FirstOrDefault();

                return new PropertyModelInfo()
                {
                    Name = info.Name,
                    Type = info.PropertyType,
                    MemberInfo = info,
                    PropertyInfo = info,
                    BackingField = backingField,
                    IsStatic = info.IsStatic(false),
                    IsPublic = info.IsPublic(false),
                    CanRead = info.CanRead(false),
                    CanWrite = info.CanWrite(false),
                    HasParameters = info.HasParameters(false),
                    HasBackingField = backingField != null,
                    Depth = depth,
                    IsLiteral = false,
                    IsAbstract = info.IsAbstract(false),
                    IsVirtual = info.IsVitrual(false),
                    IsNew = info.IsNew(false)
                };
            }

            public string Name { get; private set; }
            public Type Type { get; private set; }
            public MemberInfo MemberInfo { get; private set; }
            public PropertyInfo PropertyInfo { get; private set; }
            public IFieldModelInfo BackingField { get; private set; }
            public bool IsStatic { get; private set; }
            public bool IsPublic { get; private set; }
            public bool CanRead { get; private set; }
            public bool CanWrite { get; private set; }
            public bool HasParameters { get; private set; }
            public bool HasBackingField { get; private set; }
            public int Depth { get; private set; }
            public bool IsLiteral { get; private set; }
            public bool IsAbstract { get; private set; }
            public bool IsVirtual { get; private set; }
            public bool IsNew { get; private set; }
        }
        #endregion
    }
}
