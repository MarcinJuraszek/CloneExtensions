using System;
using System.Collections.Generic;
using CloneExtensions.UnitTests.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class ComplexTypeTests : TestBase
    {
        [TestMethod]
        public void GetClone_Class_Cloned()
        {
            var source = new SimpleClass() { Property = 10, _field = 3 };
            var target = CloneFactory.GetClone(source);
            Assert.AreEqual(source.Property, target.Property);
            Assert.AreEqual(source._field, target._field);
        }

        [TestMethod]
        public void GetClone_Struct_Cloned()
        {
            var source = new SimpleStruct() { Property = 10, _field = 3 };
            var target = CloneFactory.GetClone(source);

        }

        [TestMethod]
        public void GetClone_SameTypepProperty_Cloned()
        {
            var source = new SelfReferencedClass { Value = new SelfReferencedClass() };
            var target = CloneFactory.GetClone(source);
            Assert.IsNotNull(target.Value);
            Assert.IsNull(target.Value.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetClone_AbstractClassInitializerNotSpecified_InvalidOperationExceptionThrown()
        {
            var source = (AbstractClass)new DerivedClass() { AbstractProperty = 10 };
            var target = CloneFactory.GetClone(source);
        }

        [TestMethod]
        public void GetCLone_AbstractClassInitializerSpecified_InstanceCloned()
        {
            var source = (AbstractClass)new DerivedClass() { AbstractProperty = 10 };
            var initializers = new Dictionary<Type, Func<object, object>>() {
                { typeof(AbstractClass), (s) => new DerivedClass() }
            };
            var target = CloneFactory.GetClone(source, initializers);
            Assert.AreEqual(source.AbstractProperty, target.AbstractProperty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetClone_InterfaceInitializerNotSpecified_InvalidOperationExceptionThrown()
        {
            IInterface source = new DerivedClass() { InterfaceProperty = 10 };
            var target = CloneFactory.GetClone(source);
        }

        [TestMethod]
        public void GetClone_InterfaceInitializerSpecified_InstanceCloned()
        {
            IInterface source = new DerivedClass() { InterfaceProperty = 10};
            var initializers = new Dictionary<Type, Func<object, object>>() {
                { typeof(IInterface), (s) => new DerivedClass() }
            };
            var target = CloneFactory.GetClone(source, initializers);

            Assert.AreNotSame(source, target);
            Assert.AreEqual(source.InterfaceProperty, target.InterfaceProperty);
        }

        struct SimpleStruct
        {
            public int _field;
            public int Property { get; set; }
        }

        class SimpleClass
        {
            public int _field;
            public int Property { get; set; }
        }

        class SelfReferencedClass
        {
            public SelfReferencedClass Value { get; set; }
        }

        interface IInterface
        {
            int InterfaceProperty { get; set; }
        }

        abstract class AbstractClass
        {
            public abstract int AbstractProperty { get; set; }
        }

        class DerivedClass : AbstractClass, IInterface
        {
            public override int AbstractProperty { get; set; }

            public int InterfaceProperty { get; set; }
        }
    }
}
