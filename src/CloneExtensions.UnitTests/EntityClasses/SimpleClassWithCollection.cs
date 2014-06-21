using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneExtensions.UnitTests.EntityClasses
{
    public class SimpleClassWithCollection
    {
        public int[] Something { get; set; }
    }

    public class SimpleClassWithNonGenericArray
    {
        public Array Something { get; set; }
    }
}
