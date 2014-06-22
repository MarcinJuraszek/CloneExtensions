using CloneExtensions.UnitTests.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloneExtensions.UnitTests.Helpers;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class NullableOfTTests : TestBase
    {
        [TestMethod]
        public void GetClone_NullableIntWithValue_ValueCloned()
        {
            AssertHelpers.GetCloneAndAssert(() => (int?)10);
        }

        [TestMethod]
        public void GetClone_NullableIntWithoutValue_NullCloned()
        {
            AssertHelpers.GetCloneAndAssert(() => (int?)null);
        }
    }
}
