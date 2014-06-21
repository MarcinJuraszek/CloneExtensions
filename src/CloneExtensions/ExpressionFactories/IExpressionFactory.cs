using System.Linq.Expressions;

namespace CloneExtensions.ExpressionFactories
{
    interface IExpressionFactory<T>
    {
        bool IsDeepCloneDifferentThanShallow { get; }

        bool AddNullCheck { get; }

        Expression GetShallowCloneExpression();

        Expression GetDeepCloneExpression();
    }
}
