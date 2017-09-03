using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CloneExtensions
{
    static internal class Helpers
    {
        public static Expression GetCloningFlagsExpression(CloningFlags flags, ParameterExpression parameter)
        {
            var flagExpression = Expression.Convert(Expression.Constant(flags, typeof(CloningFlags)), typeof(byte));

            return Expression.Equal(
                Expression.And(
                    Expression.Convert(parameter, typeof(byte)),
                    flagExpression
                ),
                flagExpression
            );
        }

        public static Expression GetCloneMethodCall(Type type, Expression source, Expression flags, Expression initializers, Expression clonedObjects)
        {
            return Expression.Call(typeof(CloneFactory), "GetClone", new[] { type }, source, flags, initializers, clonedObjects);
        }

        public static Expression GetThrowInvalidOperationExceptionExpression(Type type)
        {
            var message = string.Format("You have to provide initialization expression for {0}.", type.FullName);

            return Expression.Throw(
                Expression.New(
                    typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }),
                    Expression.Constant(message, typeof(string))
                )
            );
        }

        public static T GetFromClonedObjects<T>(Dictionary<object, object> clonedObjects, T source)
        {
            object returnValue;
            clonedObjects.TryGetValue(source, out returnValue);
            return returnValue != null ? (T)returnValue : default(T);
        }
    }
}