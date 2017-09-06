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
        public void GetClone_ClassInitializerNotSpecified_InvalidOperationExceptionThrown()
        {
            NoDefaultConstructorClass source = new NoDefaultConstructorClass(10);
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
            INoDefaultConstructor source = new NoDefaultConstructorClass(10);
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

        [TestMethod]
        public void GetClone_CircularDependency_ItemsClonedCorrectly()
        {
            CircularReference1 one = new CircularReference1();
            CircularReference2 two = new CircularReference2();
            one.First = two;
            one.Second = two;
            two.Other = one;

            var target = CloneFactory.GetClone(one);

            Assert.AreSame(target.First, target.Second, "Are the same");
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
        
        class CircularReference1
        {
            public CircularReference2 First { get;set; }

            public CircularReference2 Second { get; set; }
        }

        class CircularReference2
        {
            public CircularReference1 Other { get;set; }
        }

        interface INoDefaultConstructor
        {    
        }

        class NoDefaultConstructorClass : INoDefaultConstructor
        {
            public NoDefaultConstructorClass(int propOne)
            {
                PropOne = PropOne;
            }

            private NoDefaultConstructorClass()
            {
            }

            public int PropOne { get; set; }
        }
    }
}
