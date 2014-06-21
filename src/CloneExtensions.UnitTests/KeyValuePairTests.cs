using System;
using System.Collections.Generic;
using CloneExtensions.UnitTests.EntityClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class KeyValuePairTests
    {
        [TestMethod]
        public void KeyValuePairClone_InstanceIsCloned()
        {
            var source = new KeyValuePair<int, string>(10, "test");
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source, target);
        }

        [TestMethod]
        public void KeyValuePairClone_ValueTypeKey_KeyIsCloned()
        {
            var source = new KeyValuePair<int, string>(10, "test");
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source.Value, target.Value);
        }

        [TestMethod]
        public void KeyValuePairClone_ValueTypeValue_ValueIsCloned()
        {
            var source = new KeyValuePair<int, string>(10, "test");
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source.Key, target.Key);
        }

        [TestMethod]
        public void KeyValuePairClone_RefTypeKey_KeysAreNotTheSame()
        {
            var source = new KeyValuePair<SimpleClass, int>(new SimpleClass(), 10);
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreNotSame(source.Value, target.Value);
        }

        [TestMethod]
        public void KeyValuePairClone_RefTypeValue_ValueAreNotTheSame()
        {
            var source = new KeyValuePair<int, SimpleClass>(10, new SimpleClass());
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreNotSame(source.Value, target.Value);
        }
    }
}
