using CloneExtensions.UnitTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class PolymorphismSupportTests
    {
        [TestMethod]
        public void PolymorphismSupportTests_IRealOnlyList_String()
        {
            IReadOnlyList<string> source = new List<string>()
            {
                RandGen.GenerateString(10)
            };

            var dest = source.GetClone();

            Assert.IsNotNull(dest);
            Assert.AreNotSame(dest, source);
            Assert.AreEqual(dest.Count, source.Count);
            Assert.AreEqual(dest[0], source[0]);
        }

        [TestMethod]
        public void PolymorphismSupportTests_Interface()
        {
            MyTmpInterface source = new Helper1()
            {
                PropOne = RandGen.GenerateInt(),
                PropThree = RandGen.GenerateInt(),
                PropTwo = RandGen.GenerateInt()
            };

            var target = source.GetClone();

            Assert.IsNotNull(target);
            Assert.AreNotSame(target, source);
            Assert.AreEqual(target.PropOne, source.PropOne);
            Assert.AreNotSame(target.PropOne, source.PropOne);

            Helper1 targetAsHelper = target as Helper1;
            Helper1 sourceAsHelper = source as Helper1;

            Assert.IsNotNull(targetAsHelper);
            Assert.IsNotNull(sourceAsHelper);
            Assert.AreNotSame(targetAsHelper, sourceAsHelper);
            Assert.AreEqual(targetAsHelper.PropOne, sourceAsHelper.PropOne);
            Assert.AreEqual(targetAsHelper.PropTwo, sourceAsHelper.PropTwo);
            Assert.AreEqual(targetAsHelper.PropThree, sourceAsHelper.PropThree);
            Assert.AreNotSame(targetAsHelper.PropOne, sourceAsHelper.PropOne);
            Assert.AreNotSame(targetAsHelper.PropTwo, sourceAsHelper.PropTwo);
            Assert.AreNotSame(targetAsHelper.PropThree, sourceAsHelper.PropThree);
        }

        [TestMethod]
        public void PolymorphismSupportTests_IReadOnlyList_Interface()
        {
            IReadOnlyList<MyTmpInterface> source = new List<MyTmpInterface>()
            {
                new Helper1() { PropOne = RandGen.GenerateInt() },
                new Helper1_1() { PropOne = RandGen.GenerateInt() },
            };

            var target = source.GetClone();

            Assert.IsNotNull(target);
            Assert.AreNotSame(target, source);

            Assert.IsTrue(target[0] is Helper1);
            Assert.IsTrue(target[1] is Helper1_1);
            Assert.AreEqual(target[0].PropOne, source[0].PropOne);
            Assert.AreNotSame(target[0].PropOne, source[0].PropOne);
            Assert.AreEqual(target[1].PropOne, source[1].PropOne);
            Assert.AreNotSame(target[1].PropOne, source[1].PropOne);
        }

        [TestMethod]
        public void PolymorphismSupportTests_IReadOnlyList_Abstract()
        {
            IReadOnlyList<HelperAbstract> source = new List<HelperAbstract>()
            {
                new Helper1() { PropOne = RandGen.GenerateInt() },
                new Helper1_1() { PropOne = RandGen.GenerateInt() },
            };

            var target = source.GetClone();

            Assert.IsNotNull(target);
            Assert.AreNotSame(target, source);

            Assert.IsTrue(target[0] is Helper1);
            Assert.IsTrue(target[1] is Helper1_1);
            Assert.AreEqual(target[0].PropOne, source[0].PropOne);
            Assert.AreNotSame(target[0].PropOne, source[0].PropOne);
            Assert.AreEqual(target[1].PropOne, source[1].PropOne);
            Assert.AreNotSame(target[1].PropOne, source[1].PropOne);
        }

        [TestMethod]
        public void PolymorphismSupportTests_ConcreteSubClass()
        {
            Message source = new Message()
            {
                aRef = new Derived()
                {
                    iBase = RandGen.GenerateInt(),
                    iDerived = RandGen.GenerateInt()
                }
            };

            var dest = source.GetClone();

            Assert.IsNotNull(dest);
            Assert.IsNotNull(dest.aRef);
            Assert.AreNotSame(dest, source);
            Assert.AreNotSame(dest.aRef, source.aRef);
            Assert.AreEqual(dest.aRef.iBase, source.aRef.iBase);
            Assert.AreSame(dest.aRef.GetType(), source.aRef.GetType());
            Assert.AreEqual(dest.aRef.GetType(), typeof(Derived));
        }

        [TestMethod]
        public void PolymorphismSupportTests_InitializerSupport()
        {
            // In order to remain backwards compatible, ensure 
            // that if a user supplied an initializer it is used
            // before the new polymorphism support code is.

            int callCount = 0;

            Func<object, object> initializer = (x) =>
            {
                callCount++;
                return new Helper1();
            };

            Dictionary<Type, Func<object, object>> initializers = new Dictionary<Type, Func<object, object>>();
            initializers.Add(typeof(HelperAbstract), initializer);

            HelperAbstract source = new Helper1_1()
            {
                PropOne = RandGen.GenerateInt()
            };

            var target = source.GetClone(initializers);

            Assert.IsTrue(callCount == 1);
        }

        public void PolymorphismSupportTests_SpeedComparison1()
        {
            Helper1 concreteSource = new Helper1()
            {
                PropOne = RandGen.GenerateInt(),
                PropTwo = RandGen.GenerateInt(),
                PropThree = RandGen.GenerateInt()
            };

           MyTmpInterface abstractSource = concreteSource as MyTmpInterface;

            var result = TimingHelper.ComparePerformance(
                10000000,
                10000000,
                () => concreteSource.GetClone(),
                () => abstractSource.GetClone());

            Assert.IsFalse(true, result.GetReport());
        }

        public void PolymorphismSupportTests_SpeedComparison2()
        {
            List<Helper1> concreteSource = new List<Helper1>();

            for (int i = 0; i < 10000; i++)
            {
                concreteSource.Add(new Helper1()
                {
                    PropOne = RandGen.GenerateInt(),
                    PropTwo = RandGen.GenerateInt(),
                    PropThree = RandGen.GenerateInt()
                });
            }

            IReadOnlyList<MyTmpInterface> abstractSource = concreteSource
                .OfType<MyTmpInterface>()
                .ToList();

            var result = TimingHelper.ComparePerformance(
                1000,
                1000,
                () => concreteSource.GetClone(),
                () => abstractSource.GetClone());

            Assert.IsFalse(true, result.GetReport());
        }

        #region Helpers
        interface MyTmpInterface
        {
            int PropOne { get; set; }
        }

        abstract class HelperAbstract : MyTmpInterface
        {
            public int PropOne { get; set; }
        }

        class Helper1 : HelperAbstract
        {
            public int PropTwo { get; set; }
            public int PropThree { get; set; }
        }

        class Helper1_1 : HelperAbstract
        {
        }

        class Base
        {
            public int iBase;
        }

        class Derived : Base
        {
            public int iDerived;
        }

        class Message
        {
            public Base aRef;
        }
        #endregion
    }
}