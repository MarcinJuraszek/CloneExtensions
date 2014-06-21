using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class NonClonedAttributeTests
    {
        class ClassWithAttributedMembers
        {
            [NonCloned]
            public string PropertyWithAttributes { get; set; }

            [NonCloned]
            public string FieldWithAttribute;
        }

        [TestMethod]
        public void GetClone_FieldWithNotClonedAttribute_FieldNotCloned()
        {
            var source = new ClassWithAttributedMembers() { FieldWithAttribute = "value" };
            var target = CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.AreNotEqual(source.FieldWithAttribute, target.FieldWithAttribute);
            Assert.AreEqual(default(string), target.FieldWithAttribute);
        }

        [TestMethod]
        public void GetClone_PropertyWithNotClonedAttribute_PropertyNotCloned()
        {
            var source = new ClassWithAttributedMembers() { PropertyWithAttributes = "value" };
            var target = CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.AreNotEqual(source.PropertyWithAttributes, target.PropertyWithAttributes);
            Assert.AreEqual(default(string), target.PropertyWithAttributes);
        }
    }
}
