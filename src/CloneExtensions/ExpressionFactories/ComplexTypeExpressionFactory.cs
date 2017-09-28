using Deipax.Core.Common;
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
            var fields = ModelInfo<T>
                .Fields
                .Where(x =>
                    x.CanRead &&
                    x.CanWrite &&
                    x.IsPublic &&
                    !x.IsStatic &&
                    !x.IsBackingField &&
                    x.MemberInfo.GetCustomAttributes(typeof(NonClonedAttribute), true).Count() == 0)
                .Select(x => new Member(x.MemberInfo, x.Type))
                .ToList();

            return GetMembersCloneExpression(fields.ToArray(), getItemCloneExpression);
        }

        private Expression GetPropertiesCloneExpression(Func<Type, Expression, Expression> getItemCloneExpression)
        {
            var properties = ModelInfo<T>
                .Properties
                .Where(x =>
                    x.CanRead &&
                    x.CanWrite &&
                    x.IsPublic &&
                    !x.IsStatic &&
                    !x.HasParameters &&
                    x.MemberInfo.GetCustomAttributes(typeof(NonClonedAttribute), true).Count() == 0)
                .Select(x => new
                {
                    Type = x.Type,
                    MemberInfo = x.HasBackingField ?
                        (MemberInfo)x.BackingField.FieldInfo :
                        (MemberInfo)x.PropertyInfo
                })
                .Select(x => new Member(x.MemberInfo, x.Type))
                .ToList();

            return GetMembersCloneExpression(properties.ToArray(), getItemCloneExpression);
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
    }
}