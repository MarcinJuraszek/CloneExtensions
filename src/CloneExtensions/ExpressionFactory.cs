using CloneExtensions.ExpressionFactories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CloneExtensions
{
    class ExpressionFactory<T>
    {
        private Type _type;
        private Expression _typeExpression;

        public ExpressionFactory()
        {
            _type = typeof(T);
            _typeExpression = Expression.Constant(_type, typeof(Type));

            Initialize();
        }

        public Expression<Func<T, CloningFlags, IDictionary<Type, Func<object, object>>, T>> CloneExpression { get; private set; }

        internal Func<T, CloningFlags, IDictionary<Type, Func<object, object>>, T> GetCloneFunc()
        {
            return CloneExpression == null ? null : CloneExpression.Compile();
        }

        public void Initialize()
        {
            var source = Expression.Parameter(_type, "source");
            var flags = Expression.Parameter(typeof(CloningFlags), "flags");
            var initializers = Expression.Parameter(typeof(IDictionary<Type, Func<object, object>>), "initializers");
            var target = Expression.Variable(_type, "target");

            var returnLabel = Expression.Label(_type);

            var expressionFactory = GetExpressionFactory(source, target, flags, initializers, returnLabel);

            Expression cloneLogic;
            if (expressionFactory.IsDeepCloneDifferentThanShallow)
            {
                cloneLogic =
                    Expression.IfThenElse(
                        Expression.Not(Helpers.GetCloningFlagsExpression(CloningFlags.Shallow, flags)),
                        expressionFactory.GetDeepCloneExpression(),
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
                var nullConstant = Expression.Constant(null, _type);
                cloneExpression =
                    Expression.IfThenElse(
                        Expression.Equal(source, nullConstant),
                        Expression.Assign(target, nullConstant),
                        cloneLogic
                        );
            }

            var block = Expression.Block(new[] { target }, new Expression[] { cloneExpression, Expression.Label(returnLabel, target) });

            CloneExpression =
                Expression.Lambda<Func<T, CloningFlags, IDictionary<Type, Func<object, object>>, T>>(
                    block,
                    new[] { source, flags, initializers });
        }

        private IExpressionFactory<T> GetExpressionFactory(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, LabelTarget returnLabel)
        {
            if (_type.IsPrimitiveOrKnownImmutable() || typeof(Delegate).IsAssignableFrom(_type))
            {
                return new PrimitiveTypeExpressionFactory<T>(source, target, flags, initializers, returnLabel);
            }
            else if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return new NullableExpressionFactory<T>(source, target, flags, initializers, returnLabel);
            }
            else if (_type.IsArray)
            {
                return new ArrayExpressionFactory<T>(source, target, flags, initializers, returnLabel);
            }
            else if (_type.IsGenericType &&
                (_type.GetGenericTypeDefinition() == typeof(Tuple<>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,,>)))
            {
                return new TupleExpressionFactory<T>(source, target, flags, initializers, returnLabel);
            }
            else if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                return new KeyValuePairExpressionFactory<T>(source, target, flags, initializers, returnLabel);
            }

            return new ComplexTypeExpressionFactory<T>(source, target, flags, initializers, returnLabel);
        }
    }
}
