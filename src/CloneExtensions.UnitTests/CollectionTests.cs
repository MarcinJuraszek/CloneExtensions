using System;
using System.Collections.Generic;
using System.Linq;
using CloneExtensions.UnitTests.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class CollectionTests : TestBase
    {
        [TestMethod]
        public void GetClone_ListOfInts_Cloned()
        {
            var source = Enumerable.Range(0, 10).ToList();
            var target = CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.IsTrue(source.SequenceEqual(target));
        }

        [TestMethod]
        public void GetClone_ListOfIntsCloningFlagsCollectionItemsNotProvided_ItemsNotCloned()
        {
            var source = Enumerable.Range(0, 10).ToList();
            var target = CloneFactory.GetClone(source, CloningFlags.Properties);
            Assert.AreNotSame(source, target);
            Assert.AreEqual(source.Count, target.Capacity);
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void GetClone_ListOfClass_ReferenceEqualityReturnsFalse()
        {
            var source = Enumerable.Range(1, 10).Select(x => new MyClass() { _field = x, Property = x }).ToList();
            var target = CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.IsTrue(source.SequenceEqual(target));
            Assert.IsFalse(source.Zip(target, (s, t) => new { s, t }).Any(x => ReferenceEquals(x.s, x.t)));
        }

        [TestMethod]
        public void GetClone_IListOfClass_ReferenceEqualityReturnsFalse()
        {
            IList<MyClass> source = Enumerable.Range(1, 10).Select(x => new MyClass() { _field = x, Property = x }).ToList();
            var initializers = new Dictionary<Type, Func<object, object>>() {
                { typeof(IList<MyClass>), (s) => new List<MyClass>() }
            };
            var target = CloneFactory.GetClone(source, initializers);
            Assert.AreNotSame(source, target);
            Assert.IsTrue(source.SequenceEqual(target));
            Assert.IsFalse(source.Zip(target, (s, t) => new { s, t }).Any(x => ReferenceEquals(x.s, x.t)));
        }

        [TestMethod]
        public void GetClone_NullListOfInt_NullCloned()
        {
            List<int> source = null;
            var target = CloneFactory.GetClone(source);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void GetClone_Dictionary_Cloned()
        {
            var source = new Dictionary<int, string>() { { 1, "one" }, { 2, "two" } };
            var target = CloneFactory.GetClone(source);
            Assert.IsTrue(source.SequenceEqual(target));
        }

        [TestMethod]
        public void GetClone_DerivedTypeWithShadowedProperty_ClonnedProperly()
        {
            DerivedClass source = new DerivedClass() { Property = 1 };
            ((BaseClass)source).Property = 2;

            var target = CloneFactory.GetClone(source);

            Assert.AreEqual(1, target.Property);
            Assert.AreEqual(2, ((BaseClass)target).Property);
        }

        class MyClass
        {
            public int _field;
            public int Property { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as MyClass;
                if (other == null)
                    return false;

                return other._field == _field && other.Property == Property;
            }

            public override int GetHashCode()
            {
                return _field.GetHashCode() ^ Property.GetHashCode();
            }
        }

        class BaseClass
        {
            public int Property { get; set; }
        }

        class DerivedClass : BaseClass
        {
            public new int Property { get; set; }
        }
    }
}
