using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloneExtensions.UnitTests.Helpers;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class TupleTests
    {
        private class MyClass
        {
        }

        [TestMethod]
        public void GetClone_TupleOfInt_InstanceIsCloned()
        {
            AssertHelpers.GetCloneAndAssert(() => new Tuple<int>(10));
        }

        [TestMethod]
        public void GetClone_TupleT1CloneTest_RefTypeValue_ValuesAreNotTheSame()
        {
            var source = new Tuple<MyClass>(new MyClass());
            var target = CloneFactory.GetClone(source);
            Assert.AreNotSame(source.Item1, target.Item1);
            Assert.AreNotSame(source, target);
        }

        [TestMethod]
        public void GetClone_TupleT5CloneTest_InstanceIsCloned()
        {
            var source = Tuple.Create(1, 2, 3, 4, 5);
            var target = CloneFactory.GetClone(source);
            Assert.AreEqual(source, target);
            Assert.AreNotSame(source, target);
        }

        [TestMethod]
        public void GetClone_TupleT8CloneTest_InstanceIsCloned()
        {
            var source = new Tuple<int, int, int, int, int, int, int, Tuple<int, int, int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int, int, int>(8, 9, 10));
            var target = CloneFactory.GetClone(source);
            Assert.AreEqual(source, target);
            Assert.AreNotSame(source, target);
        }
    }
}
