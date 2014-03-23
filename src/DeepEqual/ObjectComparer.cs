using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace DeepEqual
{
    public class ObjectComparer: IComparer
    {
        public int Compare(object x, object y)
        {
            string valueX = ObjectToString(x);
            string valueY = ObjectToString(y);

            int comparisonResult = string.CompareOrdinal(valueX, valueY);
            return comparisonResult;
        }

        private string ObjectToString(object x)
        {
            var serializer = new JavaScriptSerializer();
            var serializedResult = serializer.Serialize(x);

            return serializedResult;
        }
    }
}
