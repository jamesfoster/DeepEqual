using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Reflection;

namespace DeepEqual
{
    public class ObjectComparer: IComparer
    {
        public IgnoredProperties IgnoredProperties;

        public ObjectComparer(IgnoredProperties ignoredProperties)
		{
            IgnoredProperties = ignoredProperties;
		}

        public int Compare(object x, object y)
        {
            string valueX = ObjectToString(x);
            string valueY = ObjectToString(y);

            int comparisonResult = string.CompareOrdinal(valueX, valueY);
            return comparisonResult;
        }

        private string ObjectToString(object o)
        {
            Type t = o.GetType();
            List<string> ignoredProperties = IgnoredProperties.GetIgnoredPropertiesForTypes(t, t);

            var objectValue = new StringBuilder();

            foreach (PropertyInfo property in t.GetProperties())
            {
                if (ignoredProperties.Contains(property.Name))
                {
                    continue;
                }

                Type propertyType = property.PropertyType;

                // Strings are regarded as class properties, but they don't refer to other objects. 
                if ((propertyType == typeof(System.String)) ||
                    (propertyType == typeof(System.DateTimeOffset)) ||
                    (propertyType == typeof(Nullable<System.DateTimeOffset>)) ||
                    (propertyType == typeof(System.DateTime)) ||
                    (propertyType == typeof(Nullable<System.DateTime>)) ||
                    propertyType.IsPrimitive || 
                    propertyType.IsEnum)
                {
                    object propertyValue = property.GetValue(o, null);
                    objectValue.Append("___" + (propertyValue == null ? "" : propertyValue.ToString()));
                }
            }

            return objectValue.ToString();
        }
    }
}
