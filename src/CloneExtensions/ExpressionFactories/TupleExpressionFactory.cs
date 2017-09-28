using System;
using System.Globalization;
using System.Linq.Expressions;

namespace CloneExtensions.ExpressionFactories
{
    class TupleExpressionFactory<T> : DeepShallowExpressionFactoryBase<T>
    {
        Type[] _genericTypes;

        public TupleExpressionFactory(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, ParameterExpression clonedObjects)
            : base(source, target, flags, initializers, clonedObjects)
        {
            _genericTypes = typeof(T).GetGenericArguments();
        }

        public override bool AddNullCheck
        {
            get
            {
                return true;
            }
        }

        public override bool VerifyIfAlreadyClonedByReference
        {
            get
            {
                return true;
            }
        }

        protected override Expression GetCloneExpression(Func<Type, Expression, Expression> getItemCloneExpression)
        {
            var itemsCloneExpressions = new Expression[_genericTypes.Length];

            // Can't loop to 8, because instead of Item8 the last one is called Rest
            var loopCount = Math.Min(_genericTypes.Length, 7);
            for (int i = 0; i < loopCount; i++)
            {
                var itemType = _genericTypes[i];
                var sourceProperty = Expression.Property(
                    Source,
                    "Item" + (i + 1).ToString(CultureInfo.InvariantCulture));

                itemsCloneExpressions[i] = itemType.UsePrimitive() ?
                    sourceProperty : 
                    getItemCloneExpression(itemType, sourceProperty);
            }

            // add Rest expression if it's necessary
            if (_genericTypes.Length == 8)
            {
                var itemType = _genericTypes[7];
                var sourceProperty = Expression.Property(
                    Source,
                    "Rest");

                itemsCloneExpressions[7] = itemType.UsePrimitive() ?
                    sourceProperty :
                    getItemCloneExpression(itemType, sourceProperty);
            }

            var constructor = typeof(T).GetConstructors()[0];

            var assign = Expression.Assign(
                Target,
                Expression.New(constructor, itemsCloneExpressions));

            return Expression.Block(
                assign,
                GetAddToClonedObjectsExpression());
        }
    }
}