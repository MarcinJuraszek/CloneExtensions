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
        private static MethodInfo _helper = typeof(CloneItDelegateCache)
            .GetRuntimeMethod("Get", new[] { typeof(object) });

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
            var containsKey = Expression.Variable(typeof(bool));
            var containsKeyCall = Expression.Call(Initializers, "ContainsKey", null, _typeExpression);
            var assignContainsKey = Expression.Assign(containsKey, containsKeyCall);

            var ifThenElse = Expression.IfThenElse(
                Expression.Or(containsKey, Expression.TypeEqual(Source, _type)),
                GetAreSameTypeBlock(getItemCloneExpression, containsKey),
                GetAreDiffTypeBlock());

            return Expression.Block(
                new[] { containsKey },
                assignContainsKey,
                ifThenElse);
        }

        private BlockExpression GetAreSameTypeBlock(
            Func<Type, Expression, Expression> getItemCloneExpression,
            ParameterExpression containsKey)
        {
            var initialization = GetInitializationExpression(containsKey);

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

        private BlockExpression GetAreDiffTypeBlock()
        {
            var func = Expression.Variable(typeof(CloneItDelegate), "func");

            var sourceAsObject = Expression.Convert(Source, typeof(object));

            var assignFunc = Expression.Assign(
                func,
                Expression.Call(_helper, sourceAsObject));

            var assign = Expression.Assign(
                Target,
                Expression.Convert(Expression.Invoke(func, sourceAsObject, Flags, Initializers, ClonedObjects), _type));

            return Expression.Block(
                new[] { func },
                assignFunc,
                assign);
        }

        private Expression GetInitializationExpression(ParameterExpression containsKey)
        {
            // initializer delegate invoke
            var dictIndex = Expression.Property(Initializers, "Item", _typeExpression);
            var funcInvokeCall = Expression.Call(dictIndex, "Invoke", null, Expression.Convert(Source, typeof(object)));
            var initializerCall = Expression.Convert(funcInvokeCall, _type);

            // parameterless constructor
            var constructor = _type.GetConstructor(new Type[0]);

            return Expression.IfThenElse(
                containsKey,
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
            // get all public properties with public setter and getter, which are not indexed properties
            var properties = from p in _type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             let setMethod = p.GetSetMethod(false)
                             let getMethod = p.GetGetMethod(false)
                             where !p.GetCustomAttributes(typeof(NonClonedAttribute), true).Any()
                             where setMethod != null && getMethod != null && !p.GetIndexParameters().Any()
                             select new Member(p, p.PropertyType);

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

            return Expression.Block(
                new[] { enumerator, collection },
                assignToEnumerator,
                assignToCollection,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.NotEqual(moveNextCall, Expression.Constant(false, typeof(bool))),
                        Expression.Call(collection, "Add", null,
                            GetCloneMethodCall(itemType, currentProperty)),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                )
            );
        }

    }
}
