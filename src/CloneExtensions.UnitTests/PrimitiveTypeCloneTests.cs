using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CloneExtensions.UnitTests.Helpers;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class PrimitiveTypeCloneTests
    {
        [TestMethod]
        public void GetClone_Int_Cloned()
        {
            AssertHelpers.GetCloneAndAssert(() => 10);
        }

        [TestMethod]
        public void GetClone_Double_Cloned()
        {
            AssertHelpers.GetCloneAndAssert(() => 10.4);
        }

        [TestMethod]
        public void GetClone_DateTime_Cloned()
        {
            AssertHelpers.GetCloneAndAssert(() => DateTime.Now);
        }

        [TestMethod]
        public void GetClone_TimeSpan_Cloned()
        {
            AssertHelpers.GetCloneAndAssert(() => TimeSpan.FromMinutes(10.5));
        }

        [TestMethod]
        public void GetClone_NonEmptyString_Cloned()
        {
            AssertHelpers.GetCloneAndAssert(() => "my input string", assertSame: true);
        }

        [TestMethod]
        public void GetClone_EmptyString_Cloned()
        {
            AssertHelpers.GetCloneAndAssert(() => string.Empty, assertSame: true);
        }

        [TestMethod]
        public void GetClone_NullString_Cloned()
        {
            AssertHelpers.GetCloneAndAssert(() => (string)null, assertSame: true);
        }
    }
}
