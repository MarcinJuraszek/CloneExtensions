using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloneExtensions.UnitTests.Helpers;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class ArraysTests
    {
        private class MyClass
        {
            public override bool Equals(object obj)
            {
                return true;
            }
            public override int GetHashCode()
            {
                return 1;
            }
        }

        [TestMethod]
        public void GetClone_NullArrayOfInt_NullCloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => (int[])null);
        }

        [TestMethod]
        public void GetClone_EmptyArrayOfInt_ArrayCloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => new int[0]);
        }

        [TestMethod]
        public void GetClone_NonEmptyArrayOfInt_ArrayCloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => new[] { 0, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void GetClone_NonEmptyStringArray_ArrayCloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => new[] { "first string", string.Empty }, assertSame: true);
        }

        [TestMethod]
        public void GetClone_NonEmptyRefArrayShallowClone_ArrayShallowCloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => new[] { new MyClass() }, assertSame: true, flags: CloningFlags.Shallow);
        }

        [TestMethod]
        public void GetClone_NonEmptyRefArray_ArrayDeepCloned()
        {
            AssertHelpers.GetArrayCloneAndAssert(() => new[] { new MyClass() });
        }
    }
}
