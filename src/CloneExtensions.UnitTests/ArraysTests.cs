using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloneExtensions.UnitTests.Helpers;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class ArraysTests
    {
        [TestMethod]
        public void GetClone_NullArrayOfInt_Cloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => (int[])null);
        }

        [TestMethod]
        public void GetClone_EmptyArrayOfInt_Cloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => new int[0]);
        }

        [TestMethod]
        public void GetClone_NonEmptyArrayOfInt_Cloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => new[] { 0, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void GetClone_NonEmptyStringArray_Cloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => new[] { "first string", string.Empty }, assertSame: true);
        }
    }
}
