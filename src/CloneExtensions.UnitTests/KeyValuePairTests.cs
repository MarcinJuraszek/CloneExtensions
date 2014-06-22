using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class KeyValuePairTests
    {
        [TestMethod]
        public void GetClone_KeyValuePairOfIntAndString_AreNotSame()
        {
            var source = new KeyValuePair<int, string>(10, "test");
            var target = CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
        }

        [TestMethod]
        public void GetClone_KeyValuePairKeyIsCloned()
        {
            var source = new KeyValuePair<int, string>(10, "test");
            var target = CloneFactory.GetClone(source);
            Assert.AreEqual(source.Value, target.Value);
        }

        [TestMethod]
        public void GetClone_KeyValuePair_ValueIsCloned()
        {
            var source = new KeyValuePair<int, string>(10, "test");
            var target = CloneFactory.GetClone(source);
            Assert.AreEqual(source.Key, target.Key);
        }

        [TestMethod]
        public void GetClone_KeyValuePairRefTypeKey_KeysAreNotTheSame()
        {
            var source = new KeyValuePair<MyClass, int>(new MyClass(), 10);
            var target = CloneFactory.GetClone(source);
            Assert.AreNotSame(source.Value, target.Value);
        }

        [TestMethod]
        public void GetClone_KeyValuePairRefTypeValue_ValueAreNotTheSame()
        {
            var source = new KeyValuePair<int, MyClass>(10, new MyClass());
            var target = CloneFactory.GetClone(source);
            Assert.AreNotSame(source.Value, target.Value);
        }

        [TestMethod]
        public void GetClone_KeyValuePairRefTypeKeyShallowClone_KeysAreNotTheSame()
        {
            var source = new KeyValuePair<MyClass, int>(new MyClass(), 10);
            var target = CloneFactory.GetClone(source, CloningFlags.Shallow);
            Assert.AreSame(source.Key, target.Key);
        }

        [TestMethod]
        public void GetClone_KeyValuePairRefTypeValueShallowClone_ValueAreNotTheSame()
        {
            var source = new KeyValuePair<int, MyClass>(10, new MyClass());
            var target = CloneFactory.GetClone(source, CloningFlags.Shallow);
            Assert.AreSame(source.Value, target.Value);
        }

        private class MyClass
        {
        }
    }
}
