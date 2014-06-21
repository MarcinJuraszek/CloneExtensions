using CloneExtensions.UnitTests.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class DefaultCloningFlagsTests : TestBase
    {
        [TestMethod]
        public void DefaultFlags_ContainsFields_True()
        {
            CheckFlag(CloningFlags.Fields, CloneFactory.DefaultFlags);
        }

        [TestMethod]
        public void DafaultFlags_ContainsProperties_True()
        {
            CheckFlag(CloningFlags.Properties, CloneFactory.DefaultFlags);
        }

        [TestMethod]
        public void DefaultFlags_ContainsCollectionItems_True()
        {
            CheckFlag(CloningFlags.CollectionItems, CloneFactory.DefaultFlags);
        }

        [TestMethod]
        public void DefaultFlags_ContainsShallow_False()
        {
            CheckFlag(CloningFlags.Shallow, CloneFactory.DefaultFlags, false);
        }

        private void CheckFlag(CloningFlags flag, CloningFlags value, bool result = true)
        {
            Assert.AreEqual(result, (value & flag) == flag);
        }
    }
}
