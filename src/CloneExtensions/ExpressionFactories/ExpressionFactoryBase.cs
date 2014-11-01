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
        private readonly ParameterExpression _clonedObjects;

        protected ExpressionFactoryBase(ParameterExpression source, Expression target, ParameterExpression flags, ParameterExpression initializers, ParameterExpression clonedObjects)
        {
            _source = source;
            _target = target;
            _flags = flags;
            _initializers = initializers;
            _clonedObjects = clonedObjects;
        }

        protected ParameterExpression Source { get { return _source; } }
        protected ParameterExpression Flags { get { return _flags; } }
        protected ParameterExpression Initializers { get { return _initializers; } }
        protected ParameterExpression ClonedObjects {  get { return _clonedObjects; } }
        protected Expression Target { get { return _target; } }

        public abstract bool IsDeepCloneDifferentThanShallow { get; }

        public abstract bool AddNullCheck { get; }

        public abstract bool VerifyIfAlreadyClonedByReference { get; }

        public abstract Expression GetDeepCloneExpression();

        public abstract Expression GetShallowCloneExpression();

        protected Expression GetCloneMethodCall(Type type, Expression item)
        {
            return Helpers.GetCloneMethodCall(type, item, Flags, Initializers, ClonedObjects);
        }
    }
}
