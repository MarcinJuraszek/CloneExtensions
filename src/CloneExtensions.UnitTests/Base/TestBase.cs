using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests.Base
{
    [TestClass]
    public class TestBase
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CloneFactory.CustomInitializers.Clear();
        }
    }
}
