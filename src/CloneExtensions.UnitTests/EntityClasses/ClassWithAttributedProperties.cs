using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneExtensions.UnitTests.EntityClasses
{
    public class ClassWithAttributedProperties
    {
        public string NormalProperty { get; set; }

        public string NormalField;

        [NonCloned]
        public string PropertyWithAttributes { get; set; }
        [NonCloned]
        public string FieldWithAttribute;
    }
}
