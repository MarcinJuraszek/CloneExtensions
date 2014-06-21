
namespace CloneExtensions.UnitTests.EntityClasses
{
    class SimpleClass
    {
        public int _field;

        public int Property { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as SimpleClass;
            if (other == null)
                return false;

            return other._field == _field && other.Property == Property;
        }

        public override int GetHashCode()
        {
            return _field.GetHashCode() ^ Property.GetHashCode();
        }
    }
}
