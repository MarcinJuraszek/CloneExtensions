
namespace CloneExtensions.UnitTests.EntityClasses
{
    class DerivedClass : AbstractClass, IInterface
    {
        public override int AbstractProperty { get; set; }

        public int InterfaceProperty { get; set; }
    }
}
