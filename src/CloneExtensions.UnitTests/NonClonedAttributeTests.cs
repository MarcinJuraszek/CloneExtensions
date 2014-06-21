using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloneExtensions.UnitTests.EntityClasses;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class NonClonedAttributeTests
    {
        [TestMethod]
        public void FieldWithAttributeTest()
        {
            var source = new ClassWithAttributedProperties() { FieldWithAttribute = "value", NormalField = "another" };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.AreEqual(source.NormalField, target.NormalField);
            Assert.AreNotEqual(source.FieldWithAttribute, target.FieldWithAttribute);
        }

        [TestMethod]
        public void PropertyWithAttributeTest()
        {
            var source = new ClassWithAttributedProperties() { PropertyWithAttributes = "value", NormalProperty = "another" };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreNotSame(source, target);
            Assert.AreEqual(source.NormalProperty, target.NormalProperty);
            Assert.AreNotEqual(source.PropertyWithAttributes, target.PropertyWithAttributes);
        }
    }
}
