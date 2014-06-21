using CloneExtensions.UnitTests.Base;
using CloneExtensions.UnitTests.EntityClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class CloningFlagsTests : TestBase
    {

        [TestMethod]
        public void ClonePropertyOnlyTest()
        {
            var source = new SimpleClass() { _field = 10, Property = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source, CloningFlags.Fields);
            Assert.AreEqual(source._field, target._field);
            Assert.AreNotEqual(source.Property, target.Property);
            Assert.AreEqual(default(int), target.Property);
        }

        [TestMethod]
        public void CloneFieldOnlyTest()
        {
            var source = new SimpleClass() { _field = 10, Property = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source, CloningFlags.Properties);
            Assert.AreNotEqual(source._field, target._field);
            Assert.AreEqual(source.Property, target.Property);
            Assert.AreEqual(default(int), target._field);
        }

        [TestMethod]
        public void FieldsIsAPartOfDefaultCloningFlagsValueTest()
        {
            CheckFlag(CloningFlags.Fields, CloneFactory.DefaultFlags);
        }

        [TestMethod]
        public void PropertiesIsAPartOfDefaultCloningFlagsValueTest()
        {
            CheckFlag(CloningFlags.Properties, CloneFactory.DefaultFlags);
        }

        [TestMethod]
        public void CollectionItemsIsAPartOfDefaultCloningFlagsValueTest()
        {
            CheckFlag(CloningFlags.CollectionItems, CloneFactory.DefaultFlags);
        }

        [TestMethod]
        public void ShallowIsNotAPartOfDefaultCloningFlagsValueTest()
        {
            Assert.IsFalse((CloningFlags.Shallow & CloneFactory.DefaultFlags) == CloningFlags.Shallow);
        }

        private void CheckFlag(CloningFlags flag, CloningFlags value)
        {
            Assert.IsTrue((value & flag) == flag);
        }
    }
}
