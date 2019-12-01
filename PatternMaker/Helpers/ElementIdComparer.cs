using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternMaker.Helpers
{
    public class ElementIdComparer : IEqualityComparer<Element>
    {

        public bool Equals(Element x, Element y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }
            if (object.ReferenceEquals(x, null) ||
                object.ReferenceEquals(y, null))
            {
                return false;
            }
            return x.Id == y.Id;
        }

        public int GetHashCode(Element obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.Id.GetHashCode();
        }
    }
}
