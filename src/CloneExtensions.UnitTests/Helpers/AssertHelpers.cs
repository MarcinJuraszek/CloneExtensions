using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CloneExtensions.UnitTests.Helpers
{
    class AssertHelpers
    {
        public static void GetCloneAndAssert<T>(Func<T> getInstance, bool assertSame = false, CloningFlags? flags = null)
        {
            var source = getInstance();
            var target =
                flags.HasValue ?
                    CloneFactory.GetClone(source, flags.Value) :
                    CloneFactory.GetClone(source);

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
                else
                {
                    Assert.AreNotSame(source, target);
                }
            }
        }

        public static void GetArrayCloneAndAssert<T>(Func<T[]> getInstance, bool assertSame = false, CloningFlags? flags = null)
        {
            var source = getInstance();
            var target =
                flags.HasValue ?
                    CloneFactory.GetClone(source, flags.Value) :
                    CloneFactory.GetClone(source);

            if (source == null)
            {
                Assert.IsNull(target);
            }
            else
            {
                Assert.AreEqual(source.Length, target.Length);
                for(int i = 0; i < source.Length; i++)
                {
                    Assert.AreEqual(source[i], target[i]);
                    if (assertSame)
                    {
                        Assert.AreSame(source[i], target[i]);
                    }
                    else
                    {
                        Assert.AreNotSame(source[i], target[i]);
                    }
                }
            }
        }
    }
}
