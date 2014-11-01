using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CloneExtensions.ExpressionFactories
{
    abstract class DeepShallowExpressionFactoryBase<T> : ExpressionFactoryBase<T>
    {
        public DeepShallowExpressionFactoryBase(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, ParameterExpression clonedObjects)
            : base(source, target, flags, initializers, clonedObjects)
        {
        }

        public abstract override bool AddNullCheck { get; }

        public abstract override bool VerifyIfAlreadyClonedByReference { get; }

        public override bool IsDeepCloneDifferentThanShallow
        {
            get
            {
                return true;
            }
        }

        public override Expression GetDeepCloneExpression()
        {
            return GetCloneExpression(GetCloneMethodCall);
        }

        public override Expression GetShallowCloneExpression()
        {
            return GetCloneExpression(SimpleReturnItemExpression);
        }

        protected abstract Expression GetCloneExpression(Func<Type, Expression, Expression> getItemCloneExpression);

        protected Expression GetAddToClonedObjectsExpression()
        {
            return VerifyIfAlreadyClonedByReference
                   ? (Expression)Expression.Call(ClonedObjects, "Add", new Type[] { }, Source, Target)
                   : Expression.Empty();
        }

        private Expression SimpleReturnItemExpression(Type type, Expression item)
        {
            return item;
        }
    }
}
