using System;
using System.Collections.Generic;
using CloneExtensions.UnitTests.Base;
using CloneExtensions.UnitTests.EntityClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class ComplexTypeTests : TestBase
    {
        [TestMethod]
        public void ClassFieldCloneTest_FieldCloned()
        {
            var source = new SimpleClass() { _field = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source._field, target._field);
        }

        [TestMethod]
        public void ClassPropertyCloneTest_PropertyCloned()
        {
            var source = new SimpleClass { Property = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source.Property, target.Property);
        }

        [TestMethod]
        public void StructFieldCloneTest_FieldCloned()
        {
            var source = new SimpleStruct() { _field = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source._field, target._field);
        }

        [TestMethod]
        public void StructPropertyCloneTest_PropertyCloned()
        {
            var source = new SimpleStruct() { Property = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source.Property, target.Property);
        }

        [TestMethod]
        public void SelfReferenceCloneTest_InstanceCloned()
        {
            var source = new SelfReferencedClass { Value = new SelfReferencedClass() };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.IsNotNull(target.Value);
            Assert.IsNull(target.Value.Value);
        }

        [TestMethod]
        public void GenericClassFieldCloneTest_FieldCloned()
        {
            var source = new GenericClass<int> { _field = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source._field, target._field);
        }

        [TestMethod]
        public void GenericClassPropertyCloneTest_PropertyCloned()
        {
            var source = new GenericClass<int> { _field = 10, Property = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source.Property, target.Property);
        }

        [TestMethod]
        public void AbstractClassCloneTest_InitializerSpecified_InstanceCloned()
        {
            var source = (AbstractClass)new DerivedClass() { AbstractProperty = 10 };
            var initializers = new Dictionary<Type, Func<object, object>>() {
                { typeof(AbstractClass), (s) => new DerivedClass() }
            };
            var target = CloneExtensions.CloneFactory.GetClone(source, initializers);
            Assert.AreEqual(source.AbstractProperty, target.AbstractProperty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AbstractClassCloneTest_InitializerNotSpecified_InvalidOperationExceptionThrown()
        {
            var source = (AbstractClass)new DerivedClass() { AbstractProperty = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);
        }

        [TestMethod]
        public void InterfaceCloneTest_InitializerSpecified_InstanceCloned()
        {
            var source = (IInterface)new DerivedClass() { InterfaceProperty = 10 };
            CloneExtensions.CloneFactory.CustomInitializers.Add(typeof(IInterface), s => new DerivedClass());
            var target = CloneExtensions.CloneFactory.GetClone(source);
            Assert.AreEqual(source.InterfaceProperty, target.InterfaceProperty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InterfaceCloneTest_InitializerNotSpecified_InvalidOperationExceptionThrown()
        {
            var source = (IInterface)new DerivedClass() { AbstractProperty = 10 };
            var target = CloneExtensions.CloneFactory.GetClone(source);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ClassWithInterfacePropertyCloneTest_InitializerNotSpecified_InvalidOperationExceptionThrown()
        {
            var source = new ClassWithInterfaceProperty() { Property = new DerivedClass() { InterfaceProperty = 10 } };
            var target = CloneExtensions.CloneFactory.GetClone(source);
        }

        [TestMethod]
        public void ClassWithInterfacePropertyCloneTest_InitializerSpecified_InstanceCloned()
        {
            var source = new ClassWithInterfaceProperty() { Property = new DerivedClass() { InterfaceProperty = 10 } };
            var initializers = new Dictionary<Type, Func<object, object>>() {
                { typeof(IInterface), (s) => new DerivedClass() }
            };
            var target = CloneExtensions.CloneFactory.GetClone(source, initializers);
            Assert.AreNotSame(source.Property, target.Property);
            Assert.AreEqual(source.Property.InterfaceProperty, target.Property.InterfaceProperty);
        }


        [TestMethod]
        public void DerivedDerivedCloneTest_ClassClonedWithPropertyReturnType()
        {
            var source = new DerivedClass() { AbstractProperty = 10 };
            var initializers = new Dictionary<Type, Func<object, object>>() {
                { typeof(DerivedClass), (s) => new DerivedDerivedClass() }
            };
            var target = CloneExtensions.CloneFactory.GetClone(source, initializers);
            Assert.AreEqual(source.AbstractProperty, target.AbstractProperty);
        }

        [TestMethod]
        public void FuncDelegateCloneTest()
        {
            Func<int, int> source = (s) => s + 10;
            var target = source.GetClone();
            Assert.AreSame(source, target);
        }
    }
}
