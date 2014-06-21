using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CloneExtensions.UnitTests.Helpers
{
    class AssertHelpers
    {
        public static void GetCloneAndAssert<T>(Func<T> getInstance, bool assertSame = false)
        {
            var source = getInstance();
            var target = CloneFactory.GetClone(source);

            if (source == null)
            {
                Assert.IsNull(target);
            }
            else
            {
                Assert.AreEqual(source, target);
                if (assertSame)
                {
                    Assert.AreSame(source, target);
                }
            }
        }
    }
}
