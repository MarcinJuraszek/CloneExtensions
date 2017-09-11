using System;
using System.Linq.Expressions;

namespace CloneExtensions.ExpressionFactories
{
    class NullableExpressionFactory<T> : ExpressionFactoryBase<T>
    {
        private Type _structType = typeof(T).GetGenericArguments()[0];

        public NullableExpressionFactory(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, ParameterExpression clonedObjects)
            : base(source, target, flags, initializers, clonedObjects)
        {
        }

        public override bool IsDeepCloneDifferentThanShallow
        {
            get
            {
                return true;
            }
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

        public override Expression GetDeepCloneExpression()
        {
            var structType = typeof(T).GetGenericArguments()[0];

            var cloneCall = structType.UsePrimitive() ?
                Expression.Property(Source, "Value") :
                GetCloneMethodCall(structType, Expression.Property(Source, "Value"));

            var newNullable = Expression.New(typeof(T).GetConstructor(new[] { _structType }), cloneCall);

            return
                Expression.IfThenElse(
                    Expression.Equal(
                        Expression.Property(Source, "HasValue"),
                        Expression.Constant(false)),
                    Expression.Assign(Target, Expression.Constant(null, typeof(T))),
                    Expression.Assign(Target, newNullable));
        }

        public override Expression GetShallowCloneExpression()
        {
            return Expression.Assign(Target, Source);
        }
    }
}