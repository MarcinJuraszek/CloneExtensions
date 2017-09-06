using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace CloneExtensions
{
    public static class CloneItDelegateCache
    {
        static CloneItDelegateCache()
        {
            _cache = new ConcurrentDictionary<Type, CloneItDelegate>();
        }

        #region Field Members
        private static ConcurrentDictionary<Type, CloneItDelegate> _cache;

        private static MethodInfo _helper = typeof(CloneItDelegateCache)
            .GetRuntimeMethods()
            .Where(x => 
                x.Name == "CloneItDelegateHelper" &&
                x.IsPrivate &&
                x.IsStatic)
            .FirstOrDefault();
        #endregion

        #region Public Members
        public static CloneItDelegate Get(Type t)
        {
            return _cache.GetOrAdd(t, (x) =>
            {
                return (CloneItDelegate)_helper
                    .MakeGenericMethod(x)
                    .Invoke(null, null);
            });
        }

        public static CloneItDelegate Get(object source)
        {
            return Get(source.GetType());
        }
        #endregion

        #region Private Members
        private static CloneItDelegate CloneItDelegateHelper<T>()
        {
            var source = Expression.Parameter(typeof(object), "source");
            var target = Expression.Variable(typeof(object), "target");
            var flags = Expression.Parameter(typeof(CloningFlags), "flags");
            var initializers = Expression.Parameter(typeof(IDictionary<Type, Func<object, object>>), "initializers");
            var clonedObjects = Expression.Parameter(typeof(Dictionary<object, object>), "clonedObjects");

            var methodInfo = typeof(CloneManager<T>)
                .GetRuntimeMethods()
                .Where(x => 
                    x.Name == "Clone" &&
                    x.IsStatic)
                .FirstOrDefault();

            var invoke = Expression.Call(
                methodInfo,
                Expression.Convert(source, typeof(T)),
                flags,
                initializers,
                clonedObjects);

            var assign = Expression.Assign(
                target,
                Expression.Convert(invoke, typeof(object)));

            var block = Expression.Block(
                new[] {target},
                assign,
                Expression.Label(Expression.Label(typeof(object)), target));

            return Expression.Lambda<CloneItDelegate>(block, source, flags, initializers, clonedObjects).Compile();
        }
        #endregion
    }

    public delegate object CloneItDelegate(
        object source,
        CloningFlags flags,
        IDictionary<Type, Func<object, object>> initializers,
        Dictionary<object, object> clonedObjects);
}
