using System;
using System.Linq.Expressions;

namespace CloneExtensions.ExpressionFactories
{
    abstract class ExpressionFactoryBase<T> : IExpressionFactory<T>
    {
        private readonly ParameterExpression _source;
        private readonly Expression _target;
        private readonly ParameterExpression _flags;
        private readonly ParameterExpression _initializers;

        protected ExpressionFactoryBase(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers)
        {
            _source = source;
            _target = target;
            _flags = flags;
            _initializers = initializers;
        }

        protected ParameterExpression Source { get { return _source; } }
        protected ParameterExpression Flags { get { return _flags; } }
        protected ParameterExpression Initializers { get { return _initializers; } }
        protected Expression Target { get { return _target; } }

        public abstract bool IsDeepCloneDifferentThanShallow { get; }

        public abstract bool AddNullCheck { get; }

        public abstract Expression GetDeepCloneExpression();

        public abstract Expression GetShallowCloneExpression();

        protected Expression GetCloneMethodCall(Type type, Expression item)
        {
            return Helpers.GetCloneMethodCall(type, item, Flags, Initializers);
        }
    }
}
