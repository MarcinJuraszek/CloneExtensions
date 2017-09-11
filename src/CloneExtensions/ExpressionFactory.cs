using CloneExtensions.ExpressionFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CloneExtensions
{
    class ExpressionFactory<T>
    {
        private static Type _type = typeof(T);
        private static Expression _typeExpression = Expression.Constant(_type, typeof(Type));
        private static Expression nullConstant = null;

        private static ParameterExpression source = Expression.Parameter(_type, "source");
        private static ParameterExpression flags = Expression.Parameter(typeof(CloningFlags), "flags");
        private static ParameterExpression initializers = Expression.Parameter(typeof(IDictionary<Type, Func<object, object>>), "initializers");
        private static ParameterExpression clonedObjects = Expression.Parameter(typeof(Dictionary<object, object>), "clonedObjects");
        private static ParameterExpression target = Expression.Variable(_type, "target");

        static ExpressionFactory()
        {
            Initialize();
        }

        public static Expression<Func<T, CloningFlags, IDictionary<Type, Func<object, object>>, Dictionary<object, object>, T>> CloneExpression { get; private set; }

        internal Func<T, CloningFlags, IDictionary<Type, Func<object, object>>, Dictionary<object, object>, T> GetCloneFunc()
        {
            return CloneExpression.Compile();
        }

        public static void Initialize()
        {
            var returnLabel = Expression.Label(_type);

            var expressionFactory = GetExpressionFactory(source, target, flags, initializers, clonedObjects, returnLabel);

            if(expressionFactory.AddNullCheck || expressionFactory.VerifyIfAlreadyClonedByReference)
                nullConstant = Expression.Constant(null, _type);

            Expression cloneLogic;
            if (expressionFactory.IsDeepCloneDifferentThanShallow)
            {
                cloneLogic =
                    Expression.IfThenElse(
                        Expression.Not(Helpers.GetCloningFlagsExpression(CloningFlags.Shallow, flags)),
                        GetFromClonedObjectsOrCallDeepClone(expressionFactory),
                        expressionFactory.GetShallowCloneExpression()
                    );
            }
            else
            {
                cloneLogic = expressionFactory.GetDeepCloneExpression();
            }

            Expression cloneExpression = cloneLogic;
            if (expressionFactory.AddNullCheck)
            {
                cloneExpression =
                    Expression.IfThenElse(
                        Expression.Equal(source, nullConstant),
                        Expression.Assign(target, nullConstant),
                        cloneLogic
                        );
            }

            var block = Expression.Block(new[] { target },
                new Expression[] { cloneExpression, Expression.Label(returnLabel, target) });

            CloneExpression =
                Expression.Lambda<Func<T, CloningFlags, IDictionary<Type, Func<object, object>>, Dictionary<object, object>, T>>(
                    block,
                    new[] { source, flags, initializers, clonedObjects });
        }

        private static Expression GetFromClonedObjectsOrCallDeepClone(IExpressionFactory<T> expressionFactory)
        {
            if (expressionFactory.VerifyIfAlreadyClonedByReference)
            {
                var getFromCollectionCall = Expression.Call(typeof(Helpers), "GetFromClonedObjects", new[] { _type }, new Expression[] { clonedObjects, source });
                var fromClonedObjects = Expression.Variable(_type, "fromClonedObjects");
                var assignFromCall = Expression.Assign(fromClonedObjects, getFromCollectionCall);

                var ifElse =
                    Expression.IfThenElse(
                        Expression.NotEqual(fromClonedObjects, nullConstant),
                            Expression.Assign(target, fromClonedObjects),
                            expressionFactory.GetDeepCloneExpression());

                return Expression.Block(new[] { fromClonedObjects },
                    assignFromCall, ifElse);
            }
            else
            {
                return expressionFactory.GetDeepCloneExpression();
            }
        }

        private static IExpressionFactory<T> GetExpressionFactory(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, ParameterExpression clonedObjects, LabelTarget returnLabel)
        {
            if (_type.UsePrimitive())
            {
                return new PrimitiveTypeExpressionFactory<T>(source, target, flags, initializers, clonedObjects);
            }
            else if (_type.IsGenericType() && _type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return new NullableExpressionFactory<T>(source, target, flags, initializers, clonedObjects);
            }
            else if (_type.IsArray)
            {
                var itemType = _type
                    .GetInterfaces()
                    .First(x => x.IsGenericType() && x.GetGenericTypeDefinition() == typeof(ICollection<>))
                    .GetGenericArguments()
                    .First();

                if (itemType.UsePrimitive())
                {
                    return new ArrayPrimitiveTypeExpressionFactory<T>(source, target, flags, initializers, clonedObjects);
                }
                else
                {
                    return new ArrayExpressionFactory<T>(source, target, flags, initializers, clonedObjects);
                }
            }
            else if (_type.IsGenericType() &&
                (_type.GetGenericTypeDefinition() == typeof(Tuple<>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,,>)))
            {
                return new TupleExpressionFactory<T>(source, target, flags, initializers, clonedObjects);
            }
            else if (_type.IsGenericType() && _type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                return new KeyValuePairExpressionFactory<T>(source, target, flags, initializers, clonedObjects);
            }
            else if (_type.IsGenericType() && _type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = _type.GetGenericArguments()[0];

                if (itemType.UsePrimitive())
                {
                    return new ListPrimitiveTypeExpressionFactory<T>(source, target, flags, initializers, clonedObjects);
                }
            }

            return new ComplexTypeExpressionFactory<T>(source, target, flags, initializers, clonedObjects);
        }
    }
}