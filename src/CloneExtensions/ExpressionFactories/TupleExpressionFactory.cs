using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CloneExtensions.ExpressionFactories
{
    class TupleExpressionFactory<T> : DeepShallowExpressionFactoryBase<T>
    {
        Type[] _genericTypes;

        public TupleExpressionFactory(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, ParameterExpression clonedObjects)
            : base(source, target, flags, initializers, clonedObjects)
        {
            _genericTypes = typeof(T).GetTypeInfo().GetGenericArguments();
        }

        public override bool AddNullCheck
        {
            get
            {
                return false;
            }
        }

        public override bool VerifyIfAlreadyClonedByReference
        {
            get
            {
                return false;
            }
        }

        protected override Expression GetCloneExpression(Func<Type, Expression, Expression> getItemCloneExpression)
        {
            var itemsCloneExpressions = new Expression[_genericTypes.Length];

            // Can't loop to 8, because instead of Item8 the last one is called Rest
            var loopCount = Math.Min(_genericTypes.Length, 7);
            for (int i = 0; i < loopCount; i++)
            {
                itemsCloneExpressions[i] = getItemCloneExpression(
                                            _genericTypes[i],
                                            Expression.Property(
                                                Source,
                                                "Item" + (i + 1).ToString(CultureInfo.InvariantCulture)));
            }

            // add Rest expression if it's necessary
            if (_genericTypes.Length == 8)
                itemsCloneExpressions[7] = getItemCloneExpression(
                                            _genericTypes[7],
                                            Expression.Property(
                                                Source,
                                                "Rest"));

            var constructor = typeof(T).GetTypeInfo().GetConstructors()[0];

            return
                Expression.Assign(
                    Target,
                    Expression.New(
                        constructor,
                        itemsCloneExpressions
                    ));
        }
    }
}
