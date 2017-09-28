using Deipax.Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CloneExtensions.UnitTests
{
    [TestClass]
    public class ModelInfoTests
    {
        [TestMethod]
        public void ModelInfoTests_FieldTest()
        {
            AssertFields<GrandChildClass>(51, 30, 15, 12, 3, 51, 39);
            AssertFields<ChildAbstractClass>(34, 20, 10, 8, 2, 34, 26);
            AssertFields<ParentAbstractClass>(17, 10, 5, 4, 1, 17, 13);
            AssertFields<MyInterface>(0, 0, 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void ModelInfoTests_PropertyTest()
        {
            AssertProperties<GrandChildClass>(30, 18, 30, 6, 30, 24);
            AssertProperties<ChildAbstractClass>(20, 12, 20, 4, 20, 16);
            AssertProperties<ParentAbstractClass>(10, 6, 10, 2, 10, 8);
            AssertProperties<MyInterface>(2, 2, 0, 0, 2, 1);
        }

        [TestMethod]
        public void ModelInfoTests_CollectionEquivalence()
        {
            var m = ModelInfo.Create(typeof(GrandChildClass));
            Assert.IsTrue(ModelInfo<GrandChildClass>.Fields == m.Fields);
            Assert.IsTrue(ModelInfo<GrandChildClass>.Properties == m.Properties);
        }

        #region Private Members
        private static void AssertFields<T>(
            int fieldCount,
            int backingFieldCount,
            int publicFieldCount,
            int staticFieldCount,
            int literalFieldCount,
            int canReadFieldCount,
            int canWriteFieldCount)
        {
            var fields1 = ModelInfo<T>.Fields;
            var fields2 = ModelInfo<T>.Fields;

            var backingFields = fields1
                .Where(x => x.IsBackingField)
                .ToList();

            var publicFields = fields1
                .Where(x => x.IsPublic)
                .ToList();

            var staticFields = fields1
                .Where(x => x.IsStatic)
                .ToList();

            var literalFields = fields1
                .Where(x => x.IsLiteral)
                .ToList();

            var canReadFields = fields1
                .Where(x => x.CanRead)
                .ToList();

            var canWriteFields = fields1
                .Where(x => x.CanWrite)
                .ToList();

            Assert.IsTrue(fields1 == fields2);
            Assert.IsTrue(fields1.Count() == fieldCount);
            Assert.IsTrue(backingFields.Count == backingFieldCount);
            Assert.IsTrue(publicFields.Count == publicFieldCount);
            Assert.IsTrue(staticFields.Count == staticFieldCount);
            Assert.IsTrue(literalFields.Count == literalFieldCount);
            Assert.IsTrue(canReadFields.Count == canReadFieldCount);
            Assert.IsTrue(canWriteFields.Count == canWriteFieldCount);
        }

        private static void AssertProperties<T>(
            int propCount,
            int publicPropCount,
            int backingFieldCount,
            int staticPropCount,
            int canReadPropCount,
            int canWritePropCount)
        {
            var props1 = ModelInfo<T>.Properties;
            var props2 = ModelInfo<T>.Properties;

            var publicProps = props1
                .Where(x => x.IsPublic)
                .ToList();

            var backingFields = props1
                .Where(x => x.HasBackingField)
                .ToList();

            var staticProps = props1
                .Where(x => x.IsStatic)
                .ToList();

            var canReadProps = props1
                .Where(x => x.CanRead)
                .ToList();

            var canWriteProps = props1
                .Where(x => x.CanWrite)
                .ToList();

            var hasParameters = props1
                .Where(x => x.HasParameters)
                .ToList();

            Assert.IsTrue(props1 == props2);
            Assert.IsTrue(props1.Count() == propCount);
            Assert.IsTrue(publicProps.Count == publicPropCount);
            Assert.IsTrue(backingFields.Count == backingFieldCount);
            Assert.IsTrue(staticProps.Count == staticPropCount);
            Assert.IsTrue(canReadProps.Count == canReadPropCount);
            Assert.IsTrue(canWriteProps.Count == canWritePropCount);
            Assert.IsTrue(hasParameters.Count == 0);
        }
        #endregion

        #region Helpers
        class GrandChildClass : ChildAbstractClass
        {
            public new string PublicFieldCommonName;
            private string PrivateFieldCommonName;

            private string PrivatePropCommonName { get; set; }
            public new string PublicPropCommonName { get; set; }

            public string GrandChild_PublicField;
            private string GrandChild_PrivateField;
            public readonly string GrandChild_ReadOnlyField;
            public static string GrandChild_StaticField;
            public const string GrandChild_ConstField = "";

            public static string GrandChild_Public_Static_GetSet_AutoProp { get; set; }
            private static string GrandChild_Private_Static_GetSet_AutoProp { get; set; }

            public string GrandChild_Public_GetSet_AutoProp { get; set; }
            public string GrandChild_Public_GetPSet_AutoProp { get; private set; }
            public string GrandChild_Public_PGetSet_AutoProp { private get; set; }
            public string GrandChild_Public_Get_AutoProp { get; }

            private string GrandChild_Private_GetSet_AutoProp { get; set; }
            private string GrandChild_Private_Get_AutoProp { get; }
        }

        abstract class ChildAbstractClass : ParentAbstractClass
        {
            public new string PublicFieldCommonName;
            private string PrivateFieldCommonName;

            private string PrivatePropCommonName { get; set; }
            public new string PublicPropCommonName { get; set; }

            public string Child_PublicField;
            private string Child_PrivateField;
            public readonly string Child_ReadOnlyField;
            public static string Child_StaticField;
            public const string Child_ConstField = "";

            public static string Child_Public_Static_GetSet_AutoProp { get; set; }
            private static string Child_Private_Static_GetSet_AutoProp { get; set; }

            public string Child_Public_GetSet_AutoProp { get; set; }
            public string Child_Public_GetPSet_AutoProp { get; private set; }
            public string Child_Public_PGetSet_AutoProp { private get; set; }
            public string Child_Public_Get_AutoProp { get; }

            private string Child_Private_GetSet_AutoProp { get; set; }
            private string Child_Private_Get_AutoProp { get; }
        }

        abstract class ParentAbstractClass : MyInterface
        {
            public string PublicFieldCommonName;
            private string PrivateFieldCommonName;

            private string PrivatePropCommonName { get; set; }
            public string PublicPropCommonName { get; set; }

            public string Parent_PublicField;
            private string Parent_PrivateField;
            public readonly string Parent_ReadOnlyField;
            public static string Parent_StaticField;
            public const string Parent_ConstField = "";

            public static string Parent_Public_Static_GetSet_AutoProp { get; set; }
            private static string Parent_Private_Static_GetSet_AutoProp { get; set; }

            public string Parent_Public_GetSet_AutoProp { get; set; }
            public string Parent_Public_GetPSet_AutoProp { get; private set; }
            public string Parent_Public_PGetSet_AutoProp { private get; set; }
            public string Parent_Public_Get_AutoProp { get; }

            private string Parent_Private_GetSet_AutoProp { get; set; }
            private string Parent_Private_Get_AutoProp { get; }
        }

        interface MyInterface
        {
            string Parent_Public_GetSet_AutoProp { get; set; }
            string Parent_Public_Get_AutoProp { get; }
        }
        #endregion
    }
}
