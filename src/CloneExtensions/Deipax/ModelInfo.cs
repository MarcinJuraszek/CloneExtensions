using CloneExtensions;
using Deipax.Core.Extensions;
using Deipax.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Deipax.Core.Common
{
    public class ModelInfo
    {
        static ModelInfo()
        {
            _cache = new ConcurrentDictionary<Type, ModelInfo>();
        }

        private ModelInfo(Type t)
        {
            this.Type = t;
            this.Fields = GetAllFields(this.Type);
            this.Properties = GetAllProperties(this, this.Type);
        }

        private ModelInfo()
        {
        }

        #region Field Members
        private static ConcurrentDictionary<Type, ModelInfo> _cache;
        #endregion

        #region Public Members
        public static ModelInfo Create(Type t)
        {
            if (t != null)
            {
                return _cache.GetOrAdd(t, (x) =>
                {
                    return new ModelInfo(x);
                });
            }

            return null;
        }

        public Type Type { get; private set; }
        public IReadOnlyList<IFieldModelInfo> Fields { get; private set; }
        public IReadOnlyList<IPropertyModelInfo> Properties { get; private set; }
        #endregion

        #region Private Members
        private IFieldModelInfo GetBackingField(PropertyInfo info, int depth)
        {
            string key = string.Format("<{0}>k__BackingField", info.Name);

            return this.Fields
                .Where(x =>
                    x.IsBackingField &&
                    string.Equals(x.FieldInfo.Name, key) &&
                    x.FieldInfo.DeclaringType == info.DeclaringType &&
                    x.Depth == depth)
                .FirstOrDefault();
        }

        private static IReadOnlyList<IFieldModelInfo> GetAllFields(Type t, int depth = 0)
        {
            if (t == null)
            {
                return new List<IFieldModelInfo>();
            }

            var fields = t
                .GetTypeInfo()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Where(x => x.DeclaringType == t)
                .Select(x => FieldModelInfo.Create(x, depth))
                .ToList();

            fields.AddRange(GetAllFields(t.BaseType(), ++depth));

            return fields;
        }

        private static IReadOnlyList<IPropertyModelInfo> GetAllProperties(ModelInfo m, Type t, int depth = 0)
        {
            if (t == null)
            {
                return new List<IPropertyModelInfo>();
            }

            var props = t
                .GetTypeInfo()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Where(x => x.DeclaringType == t)
                .Select(x => PropertyModelInfo.Create(m, x, depth))
                .ToList();

            props.AddRange(GetAllProperties(m, t.BaseType(), ++depth));

            return props;
        }
        #endregion

        #region Helpers
        [DebuggerDisplay("{Name} - {IsStatic} - {IsPublic} - {CanRead} - {CanWrite} - {IsBackingField} - {Depth}")]
        class FieldModelInfo : IFieldModelInfo
        {
            public static IFieldModelInfo Create(FieldInfo info, int depth)
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

        [DebuggerDisplay("{Name} - {IsStatic} - {IsPublic} - {CanRead} - {CanWrite} - {HasBackingField} - {Depth}")]
        class PropertyModelInfo : IPropertyModelInfo
        {
            public static IPropertyModelInfo Create(ModelInfo m, PropertyInfo info, int depth)
            {
                var backingField = m.GetBackingField(info, depth);

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
                    IsLiteral = false
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
        }
        #endregion
    }

    public static class ModelInfo<T>
    {
        static ModelInfo()
        {
            var modelInfo = ModelInfo.Create(typeof(T));
            Fields = modelInfo.Fields;
            Properties = modelInfo.Properties;
        }

        #region Public Members
        public static IReadOnlyList<IFieldModelInfo> Fields { get; private set; }
        public static IReadOnlyList<IPropertyModelInfo> Properties { get; private set; }
        #endregion
    }
}