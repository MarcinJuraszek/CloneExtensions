using System;
using System.Linq.Expressions;

namespace CloneExtensions.ExpressionFactories
{
    abstract class DeepShallowExpressionFactoryBase<T> : ExpressionFactoryBase<T>
    {
        public DeepShallowExpressionFactoryBase(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, LabelTarget returnLabel)
            : base(source, target, flags, initializers, returnLabel)
        {
        }

        public abstract override bool AddNullCheck { get; }

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

        private Expression SimpleReturnItemExpression(Type type, Expression item)
        {
            return item;
        }
    }
}
