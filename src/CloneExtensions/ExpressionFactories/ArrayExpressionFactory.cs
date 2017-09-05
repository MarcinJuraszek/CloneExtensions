using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CloneExtensions.ExpressionFactories
{
    class ArrayExpressionFactory<T> : DeepShallowExpressionFactoryBase<T>
    {
        private Type _itemType;
        private Expression _arrayLength;
        protected Expression _newArray;

        public ArrayExpressionFactory(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, ParameterExpression clonedObjects)
            : base(source, target, flags, initializers, clonedObjects)
        {
            _itemType = GetItemType();
            _arrayLength = Expression.Property(Source, "Length");
            _newArray = Expression.NewArrayBounds(_itemType, _arrayLength);
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
            var counter = Expression.Variable(typeof(int));
            var breakLabel = Expression.Label();

            return Expression.Block(
                new[] { counter },
                Expression.Assign(Target, _newArray),
                GetAddToClonedObjectsExpression(),
                Expression.Assign(counter, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(counter, _arrayLength),
                        Expression.Block(
                            Expression.Assign(
                                Expression.ArrayAccess(Target, counter),
                                getItemCloneExpression(_itemType, Expression.ArrayAccess(Source, counter))
                            ),
                            Expression.AddAssign(counter, Expression.Constant(1))
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                )
            );
        }

        private static Type GetItemType()
        {
            return typeof(T).GetInterfaces()
                            .First(x => x.IsGenericType() && x.GetGenericTypeDefinition() == typeof(ICollection<>))
                            .GetGenericArguments()
                            .First();
        }
    }
}