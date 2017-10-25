using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CloneExtensions.ExpressionFactories
{
    class ComplexTypeExpressionFactory<T> : DeepShallowExpressionFactoryBase<T>
    {
        private static Type _objectType = typeof(object);

        Type _type;
        Expression _typeExpression;

        public ComplexTypeExpressionFactory(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, ParameterExpression clonedObjects)
            : base(source, target, flags, initializers, clonedObjects)
        {
            _type = typeof(T);
            _typeExpression = Expression.Constant(_type, typeof(Type));   
        }

        public override bool AddNullCheck
        {
            get
            {
                return !_type.IsValueType();
            }
        }

        public override bool VerifyIfAlreadyClonedByReference
        {
            get
            {
                return !_type.IsValueType();
            }
        }

        protected override Expression GetCloneExpression(Func<Type, Expression, Expression> getItemCloneExpression)
        {
            var initialization = GetInitializationExpression();
            var fields = 
                Expression.IfThen(
                    Helpers.GetCloningFlagsExpression(CloningFlags.Fields, Flags),
                    GetFieldsCloneExpression(getItemCloneExpression)
                );
            var properties =
                Expression.IfThen(
                    Helpers.GetCloningFlagsExpression(CloningFlags.Properties, Flags),
                    GetPropertiesCloneExpression(getItemCloneExpression)
                );

            var collectionItems = GetCollectionItemsExpression(getItemCloneExpression);

            return Expression.Block(initialization, GetAddToClonedObjectsExpression(), fields, properties, collectionItems);
        }

        private Expression GetInitializationExpression()
        {
            // initializers.ContainsKey method call
            var containsKeyCall = Expression.Call(Initializers, "ContainsKey", null, _typeExpression);

            // initializer delegate invoke
            var dictIndex = Expression.Property(Initializers, "Item", _typeExpression);
            var funcInvokeCall = Expression.Call(dictIndex, "Invoke", null, Expression.Convert(Source, typeof(object)));
            var initializerCall = Expression.Convert(funcInvokeCall, _type);

            // parameterless constructor
            var constructor = _type.GetConstructor(new Type[0]);

            return Expression.IfThenElse(
                containsKeyCall,
                Expression.Assign(Target, initializerCall),
                (_type.IsAbstract() || _type.IsInterface() || (!_type.IsValueType() && constructor == null)) ?
                    Helpers.GetThrowInvalidOperationExceptionExpression(_type) :
                    Expression.Assign(
                        Target,
                        _type.IsValueType() ? (Expression)Source : Expression.New(_type)
                    )
            );
        }

        private Expression GetFieldsCloneExpression(Func<Type, Expression, Expression> getItemCloneExpression)
        {
            var fields = from f in _type.GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.Instance)
                         where !f.GetCustomAttributes(typeof(NonClonedAttribute), true).Any()
                         where !f.IsInitOnly
                         select new Member(f, f.FieldType);

            return GetMembersCloneExpression(fields.ToArray(), getItemCloneExpression);
        }

        private Expression GetPropertiesCloneExpression(Func<Type, Expression, Expression> getItemCloneExpression)
        {
            // get all private fields with `>k_BackingField` in case we can use them instead of automatic properties
            var backingFields = GetBackingFields(_type).ToDictionary(f => new BackingFieldInfo(f.DeclaringType, f.Name));

            // use the backing fields if available, otherwise use property
            var members = new List<Member>();
            var properties = GetProperties(_type);
            foreach (var property in properties)
            {
                FieldInfo fieldInfo;
                if (backingFields.TryGetValue(new BackingFieldInfo(property.DeclaringType, "<" + property.Name + ">k__BackingField"), out fieldInfo))
                {
                    members.Add(new Member(fieldInfo, fieldInfo.FieldType));
                }
                else
                {
                    members.Add(new Member(property, property.PropertyType));
                }
            }

            return GetMembersCloneExpression(members.ToArray(), getItemCloneExpression);
        }

        private Expression GetMembersCloneExpression(Member[] members, Func<Type, Expression, Expression> getItemCloneExpression)
        {
            if (!members.Any())
                return Expression.Empty();

            return Expression.Block(
                members.Select(m =>
                    Expression.Assign(
                        Expression.MakeMemberAccess(Target, m.Info),
                        m.Type.UsePrimitive() ?
                            Expression.MakeMemberAccess(Source, m.Info) :
                            getItemCloneExpression(m.Type, Expression.MakeMemberAccess(Source, m.Info))
                        )));
        }

        private Expression GetCollectionItemsExpression(Func<Type, Expression, Expression> getItemCloneExpression)
        {
            var collectionType = _type.GetInterfaces()
                                      .FirstOrDefault(x => x.IsGenericType() && x.GetGenericTypeDefinition() == typeof(ICollection<>));
            if (collectionType == null)
                return Expression.Empty();

            return Expression.IfThen(
                Helpers.GetCloningFlagsExpression(CloningFlags.CollectionItems, Flags),
                GetForeachAddExpression(collectionType));
        }

        private Expression GetForeachAddExpression(Type collectionType)
        {
            var collection = Expression.Variable(collectionType);
            var itemType = collectionType.GetGenericArguments().First();
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(itemType);
            var enumerator = Expression.Variable(enumeratorType);
            var getEnumeratorCall = Expression.Call(Expression.Convert(Source, enumerableType), "GetEnumerator", null);
            var assignToEnumerator = Expression.Assign(enumerator, Expression.Convert(getEnumeratorCall, enumeratorType));
            var assignToCollection = Expression.Assign(collection, Expression.Convert(Target, collectionType));
            var moveNextCall = Expression.Call(enumerator, typeof(IEnumerator).GetTypeInfo().GetMethod("MoveNext"));
            var currentProperty = Expression.Property(enumerator, "Current");
            var breakLabel = Expression.Label();

            var cloneItemCall = itemType.UsePrimitive() ? 
                currentProperty : 
                GetCloneMethodCall(itemType, currentProperty);

            return Expression.Block(
                new[] { enumerator, collection },
                assignToEnumerator,
                assignToCollection,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.NotEqual(moveNextCall, Expression.Constant(false, typeof(bool))),
                        Expression.Call(collection, "Add", null, cloneItemCall),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                )
            );
        }

        private static IEnumerable<FieldInfo> GetBackingFields(Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();

            while(typeInfo != null && typeInfo.UnderlyingSystemType != _objectType)
            {
                foreach (var field in typeInfo.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.Name.Contains(">k__BackingField") && field.DeclaringType == typeInfo.UnderlyingSystemType)
                        yield return field;
                }

                typeInfo = typeInfo.BaseType?.GetTypeInfo();
            }
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();

            var allProperties = 
                (from p in GetAllProperties(type)
                let setMethod = p.GetSetMethod(false)
                select new
                {
                    Prop = p,
                    IsVirtualOrAbstract = setMethod.IsAbstract || setMethod.IsVirtual
                }).ToList();

            // If properties that share a name are marked as abstract
            // or virtual, then only one of them is needed in order to
            // set the value of the property.  Any of them should 
            // set the value correctly, use the first one.
            allProperties
                .Where(x => x.IsVirtualOrAbstract)
                .Select(x => x.Prop)
                .GroupBy(x => x.Name)
                .ToList()
                .ForEach(x => properties.Add(x.First()));

            // Add all non-virtual/non-abstract properties
            properties.AddRange(allProperties.Where(x => !x.IsVirtualOrAbstract).Select(x => x.Prop));

            return properties;
        }

        private static List<PropertyInfo> GetAllProperties(Type type)
        {
            if (type == null)
            {
                return new List<PropertyInfo>();
            }

            var typeInfo = type.GetTypeInfo();

            var properties = 
                (from p in typeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                let setMethod = p.GetSetMethod(false)
                let getMethod = p.GetGetMethod(false)
                where !p.GetCustomAttributes(typeof(NonClonedAttribute), true).Any()
                where setMethod != null && getMethod != null && !p.GetIndexParameters().Any()
                select p).ToList();

            properties.AddRange(GetAllProperties(typeInfo.BaseType));
            return properties;
        }

        private struct BackingFieldInfo : IEquatable<BackingFieldInfo>
        {
            public Type DeclaredType { get; }
            public string Name { get; set; }

            public BackingFieldInfo(Type declaringType, string name) : this()
            {
                DeclaredType = declaringType;
                Name = name;
            }

            public bool Equals(BackingFieldInfo other)
            {
                return other.DeclaredType == this.DeclaredType && other.Name == this.Name;
            }

            public override int GetHashCode()
            {
                return (17 * 23 + DeclaredType.GetHashCode()) * 23 + Name.GetHashCode();
            }
        }
    }
}