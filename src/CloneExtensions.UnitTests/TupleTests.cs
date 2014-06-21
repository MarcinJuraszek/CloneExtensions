using System;
using CloneExtensions.UnitTests.EntityClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class TupleTests
    {
        [TestMethod]
        public void TupleT1CloneTest_InstanceIsCloned()
        {
            var source = new Tuple<int>(10);
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source, target);
            Assert.AreNotSame(source, target);
        }

        [TestMethod]
        public void TupleT1CloneTest_RefTypeValue_ValuesAreNotTheSame()
        {
            var source = new Tuple<SimpleClass>(new SimpleClass());
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreNotSame(source.Item1, target.Item1);
            Assert.AreNotSame(source, target);
        }

        [TestMethod]
        public void TupleT5CloneTest_InstanceIsCloned()
        {
            var source = Tuple.Create(10, 10, 10, 10);
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source, target);
            Assert.AreNotSame(source, target);
        }

        [TestMethod]
        public void TupleT8CloneTest_InstanceIsCloned()
        {
            var source = new Tuple<int, int, int, int, int, int, int, Tuple<int, int, int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int, int, int>(8, 9, 10));
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source, target);
            Assert.AreNotSame(source, target);
        }
    }
}
