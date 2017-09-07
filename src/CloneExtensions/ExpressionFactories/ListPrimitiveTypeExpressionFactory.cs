using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CloneExtensions.ExpressionFactories
{
    class ListPrimitiveTypeExpressionFactory<T> : DeepShallowExpressionFactoryBase<T>
    {
        private static Type _type = typeof(T);
        private static ConstructorInfo _constructor = _type
            .GetConstructors()
            .Where(x =>
                x.GetParameters().Length == 1 &&
                x.GetParameters().ElementAt(0).ParameterType.IsGenericType())
            .FirstOrDefault();

        private static ConstructorInfo _capacityContructor = _type
            .GetConstructors()
            .Where(x =>
                x.GetParameters().Length == 1 &&
                x.GetParameters().ElementAt(0).ParameterType == typeof(int))
            .FirstOrDefault();

        public ListPrimitiveTypeExpressionFactory(
            ParameterExpression source,
            Expression target,
            ParameterExpression flags,
            ParameterExpression initializers,
            ParameterExpression clonedObjects)
            : base(source, target, flags, initializers, clonedObjects)
        {
        }

        public override bool AddNullCheck
        {
            get { return true; }
        }

        public override bool VerifyIfAlreadyClonedByReference
        {
            get { return true; }
        }

        protected override Expression GetCloneExpression(Func<Type, Expression, Expression> getItemCloneExpression)
        {
            var ifThenElse = Expression.IfThenElse(
                Helpers.GetCloningFlagsExpression(CloningFlags.CollectionItems, Flags),
                Expression.Assign(Target, Expression.New(_constructor, Source)),
                Expression.Assign(Target, Expression.New(_capacityContructor, Expression.Property(Source, "Count"))));

            return Expression.Block(
                ifThenElse,
                GetAddToClonedObjectsExpression());
        }
    }
}