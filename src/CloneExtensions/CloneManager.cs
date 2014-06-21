using System;
using System.Collections.Generic;

namespace CloneExtensions
{
    internal static class CloneManager<T>
    {
        private static Func<T, CloningFlags, IDictionary<Type, Func<object, object>>, T> _clone;

        private static readonly IDictionary<Type, Func<object, object>> _emptyCustomInitializersDictionary = new Dictionary<Type, Func<object, object>>();

        static CloneManager()
        {
            var factory = new ExpressionFactory<T>();
            _clone = factory.GetCloneFunc();
        }

        public static T Clone(T source, CloningFlags flags)
        {
            return _clone(source, flags, _emptyCustomInitializersDictionary);
        }

        public static T Clone(T source, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers)
        {
            if (initializers == null)
                throw new ArgumentNullException();

            return _clone(source, flags, initializers);
        }
    }
}
