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

        [TestMethod]
        public void GetClone_DerivedTypeWithShadowedProperty_ClonnedProperly()
        {
            DerivedClassOne source = new DerivedClassOne()
            {
                MyField = 1,
                Property = 1,
                VirtualProperty = 2,
                VirtualProperty2 = 3,
                AbstractProperty = 4,
                VirtualProperty3 = "test1"
            };

            Assert.AreEqual(2, ((BaseClassOne)source).VirtualProperty);
            Assert.AreEqual(3, ((BaseClassOne)source).VirtualProperty2);
            Assert.AreEqual(4, ((BaseClassOne)source).AbstractProperty);
            Assert.AreEqual("test1", ((BaseClassOne)source).VirtualProperty3);

            ((BaseClassOne) source).MyField = 2;
            ((BaseClassOne)source).Property = 5;
            ((BaseClassOne)source).VirtualProperty = 6;
            ((BaseClassOne)source).VirtualProperty2 = 7;
            ((BaseClassOne)source).AbstractProperty = 8;
            ((BaseClassOne)source).VirtualProperty3 = "test2";

            Assert.AreEqual(1, source.MyField);
            Assert.AreEqual(2, ((BaseClassOne)source).MyField);

            Assert.AreEqual(1, source.Property);
            Assert.AreEqual(5, ((BaseClassOne)source).Property);

            Assert.AreEqual(6, source.VirtualProperty);
            Assert.AreEqual(6, ((BaseClassOne)source).VirtualProperty);

            Assert.AreEqual(7, source.VirtualProperty2);
            Assert.AreEqual(7, ((BaseClassOne)source).VirtualProperty2);

            Assert.AreEqual(8, source.AbstractProperty);
            Assert.AreEqual(8, ((BaseClassOne)source).AbstractProperty);

            Assert.AreEqual("test2", source.VirtualProperty3);
            Assert.AreEqual("test2", ((BaseClassOne)source).VirtualProperty3);

            var target = CloneFactory.GetClone(source);

            Assert.AreEqual(1, target.MyField);
            Assert.AreEqual(2, ((BaseClassOne)target).MyField);

            Assert.AreEqual(1, target.Property);
            Assert.AreEqual(5, ((BaseClassOne)target).Property);

            Assert.AreEqual(6, target.VirtualProperty);
            Assert.AreEqual(6, ((BaseClassOne)target).VirtualProperty);

            Assert.AreEqual(7, target.VirtualProperty2);
            Assert.AreEqual(7, ((BaseClassOne)target).VirtualProperty2);

            Assert.AreEqual(8, target.AbstractProperty);
            Assert.AreEqual(8, ((BaseClassOne)target).AbstractProperty);

            Assert.AreEqual("test2", target.VirtualProperty3);
            Assert.AreEqual("test2", ((BaseClassOne)target).VirtualProperty3);
        }

        [TestMethod]
        public void GetClone_DerivedTypeWithShadowedProperty_ClonnedProperly2()
        {
            D source = new D()
            {
                Foo = "D"
            };

            ((C)source).Foo = "C";
            ((B)source).Foo = "B";
            ((A)source).Foo = "A";

            Assert.AreEqual("C", source.Foo);
            Assert.AreEqual("C", ((C)source).Foo);
            Assert.AreEqual("A", ((B)source).Foo);
            Assert.AreEqual("A", ((A)source).Foo);

            var target = source.GetClone();

            Assert.AreEqual("C", target.Foo);
            Assert.AreEqual("C", ((C)target).Foo);
            Assert.AreEqual("A", ((B)target).Foo);
            Assert.AreEqual("A", ((A)target).Foo);
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

        abstract class BaseClassOne : IInterface
        {
            public int MyField;

            public int Property { get; set; }
            virtual public int VirtualProperty { get; set; }
            abstract public int AbstractProperty { get; set; }
            virtual public int VirtualProperty2 { get; set; }

            virtual public string VirtualProperty3
            {
                get { return _virtualProperty; }
                set { _virtualProperty = string.Empty; }
            }

            public int InterfaceProperty { get; set; }

            private string _virtualProperty;
        }

        class DerivedClassOne : BaseClassOne
        {
            public new int MyField;

            public new int Property { get; set; }
            public override int AbstractProperty { get; set; }
            public override int VirtualProperty { get; set; }
            // use the default implementation for VirtualProperty2
            public override string VirtualProperty3 { get; set; }
        }

        public class A
        {
            public virtual string Foo { get; set; }
        }

        public class B : A
        {
            public override string Foo { get; set; }
        }

        public class C : B
        {
            public virtual new string Foo { get; set; }
        }

        public class D : C
        {
            public override string Foo { get; set; }
        }
    }
}
