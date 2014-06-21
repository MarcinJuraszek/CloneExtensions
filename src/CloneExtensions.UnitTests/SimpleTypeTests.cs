using System;
using CloneExtensions.UnitTests.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class SimpleTypeTests : TestBase
    {
        [TestMethod]
        public void IntCloneTest()
        {
            SimpleTypeTest(() => 10);
        }

        [TestMethod]
        public void DateTimeCloneTest()
        {
            SimpleTypeTest(() => DateTime.Now);
        }

        [TestMethod]
        public void StringCloneTest()
        {
            SimpleTypeTest(() => "my input string");
        }

        public static void SimpleTypeTest<T>(Func<T> factoryMethod)
        {
            var source = factoryMethod();
            var target = CloneExtensions.CloneFactory.GetClone(source);

            if (source == null)
                Assert.IsNull(target);
            else
                Assert.AreEqual(source, target);
        }
    }
}
