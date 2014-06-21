using System;
using CloneExtensions.UnitTests.Base;
using CloneExtensions.UnitTests.EntityClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class NullableTests : TestBase
    {
        [TestMethod]
        public void NullableIntCloneTest_ValueIsSet_ValueIsCloned()
        {
            SimpleTypeTests.SimpleTypeTest(() => (int?)10);
        }

        [TestMethod]
        public void NullableDateTimeCloneTest_ValueIsNull_NullIsCloned()
        {
            SimpleTypeTests.SimpleTypeTest(() => (DateTime?)null);
        }

        [TestMethod]
        public void NullableStructCloneTest_ValueIsNull_NullIsCloned()
        {
            var source = (SimpleStruct?)null;
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.IsFalse(target.HasValue);
        }

        [TestMethod]
        public void NullableStructCloneTest_ValueIsSet_ValueIsCloned()
        {
            var source = (SimpleStruct?)new SimpleStruct() { _field = 10, Property = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);

            Assert.IsNotNull(target);
            Assert.IsTrue(target.HasValue);
            Assert.AreEqual(source.Value._field, target.Value._field);
            Assert.AreNotSame(source.Value, target.Value);
        }

        [TestMethod]
        public void NullableStructCloneTest_RefPropertyIsSet_ValueIsCloned()
        {
            var source = (SimpleStruct?)new SimpleStruct() { _field = 10, Property = 10, RefProperty = new SimpleClass() };
            var target = CloneExtensions.CloneFactory.GetClone(source);

            Assert.IsNotNull(target);
            Assert.IsTrue(target.HasValue);
            Assert.AreEqual(source.Value._field, target.Value._field);
            Assert.AreNotSame(source.Value.RefProperty, target.Value.RefProperty);
        }
    }
}
