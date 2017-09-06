using CloneExtensions.UnitTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class CloneItDelegateCacheTests
    {
        [TestMethod]
        public void CloneItDelegateCacheTests_IReadOnlyList_Int()
        {
            IReadOnlyList<int> source = new List<int>()
            {
                RandGen.GenerateInt()
            };

            var cloneItDelegate = CloneItDelegateCache.Get(source);

            var flags = CloningFlags.Fields | CloningFlags.Properties | CloningFlags.CollectionItems;
            var initializers = new Dictionary<Type, Func<object, object>>();
            var clonedObjects = new Dictionary<object, object>();

            var target = cloneItDelegate(source, flags, initializers, clonedObjects);

            var targetAsList = target as List<int>;

            Assert.IsNotNull(targetAsList);
            Assert.AreNotSame(targetAsList, source);
            Assert.AreEqual(targetAsList.Count, source.Count);
            Assert.AreEqual(targetAsList[0], source[0]);
            Assert.AreNotSame(targetAsList[0], source[0]);
        }
    }
}