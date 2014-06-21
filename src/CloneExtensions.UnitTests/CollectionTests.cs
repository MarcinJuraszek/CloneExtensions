using System;
using System.Collections.Generic;
using System.Linq;
using CloneExtensions.UnitTests.Base;
using CloneExtensions.UnitTests.EntityClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class CollectionTests : TestBase
    {
        [TestMethod]
        public void ListOfIntCloneTest()
        {
            var source = Enumerable.Range(0, 10).ToList();
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.IsTrue(source.SequenceEqual(target));
        }

        [TestMethod]
        public void ListOfIntCloneTest_CloningFlagsCollectionItemsNotProvided_ItemsNotCloned()
        {
            var source = Enumerable.Range(0, 10).ToList();
            var target = CloneExtensions.CloneFactory.GetClone(source, CloningFlags.Properties);
            Assert.AreNotSame(source, target);
            Assert.AreEqual(source.Capacity, target.Capacity);
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void ListOfClassCloneTest_ReferenceEqualityReturnsFalse()
        {
            var source = Enumerable.Range(1, 10).Select(x => new SimpleClass() { _field = x, Property = x }).ToList();
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.IsTrue(source.SequenceEqual(target));
            Assert.IsFalse(source.Zip(target, (s, t) => new { s, t }).Any(x => object.ReferenceEquals(x.s, x.t)));
        }

        [TestMethod]
        public void IListOfClassCloneTest_ReferenceEqualityReturnsFalse()
        {
            IList<SimpleClass> source = Enumerable.Range(1, 10).Select(x => new SimpleClass() { _field = x, Property = x }).ToList();
            var initializers = new Dictionary<Type, Func<object, object>>() {
                { typeof(IList<SimpleClass>), (s) => new List<SimpleClass>() }
            };
            var target = CloneExtensions.CloneFactory.GetClone(source, initializers);
            Assert.AreNotSame(source, target);
            Assert.IsTrue(source.SequenceEqual(target));
            Assert.IsFalse(source.Zip(target, (s, t) => new { s, t }).Any(x => object.ReferenceEquals(x.s, x.t)));
        }

        [TestMethod]
        public void NullCloneTest()
        {
            List<int> source = null;
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void ArrayOfIntCloneTest()
        {
            var source = Enumerable.Range(0, 10).ToArray();
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.IsFalse(object.ReferenceEquals(source, target));
        }

        [TestMethod]
        public void DictionaryCloneTest()
        {
            var source = new Dictionary<int, string>() { { 1, "one" }, { 2, "two" } };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.IsTrue(source.SequenceEqual(target));
        }

        [TestMethod]
        public void NullCollectionCloneTest()
        {
            var source = new SimpleClassWithCollection() { Something = null };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.IsNull(target.Something);
        }

        [TestMethod]
        public void NonGenericCollectionTest()
        {
            var source = new SimpleClassWithNonGenericArray() { Something = null };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.IsNull(target.Something);
        }
    }
}
