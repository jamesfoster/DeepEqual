using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DeepEqual
{
    public class IgnoredProperties
    {
        public IDictionary<Type, List<string>> IgnoredPropertiesPerType { get; set; }
        public IList<string> IgnoredPropertyNames { get; set; }

        public IgnoredProperties()
        {
            IgnoredPropertiesPerType = new Dictionary<Type, List<string>>();
            IgnoredPropertyNames = new List<string>();
        }

        public void IgnoreProperty(string propertyName)
        {
            IgnoredPropertyNames.Add(propertyName);
        }

        public void IgnoreProperty<T>(Expression<Func<T, object>> property)
        {
            var exp = property.Body;

            if (exp is UnaryExpression)
            {
                exp = ((UnaryExpression)exp).Operand; // implicit cast to object
            }

            var member = exp as MemberExpression;

            if (member == null)
            {
                return;
            }

            var propertyName = member.Member.Name;

            IgnoreProperty(typeof(T), propertyName);
        }

        private void IgnoreProperty(Type type, string propertyName)
        {
            if (!IgnoredPropertiesPerType.ContainsKey(type))
            {
                IgnoredPropertiesPerType[type] = new List<string>();
            }

            IgnoredPropertiesPerType[type].Add(propertyName);
        }

        public List<string> GetIgnoredPropertiesForTypes(Type type1, Type type2)
        {
            var seed = new List<string>().AsEnumerable();

            List<string> ignoredPropertiesForTypes =
                IgnoredPropertiesPerType
                    .Where(pair => pair.Key.IsAssignableFrom(type1) || pair.Key.IsAssignableFrom(type2))
                    .Aggregate(seed, (current, pair) => current.Union(pair.Value))
                    .ToList();

            return ignoredPropertiesForTypes.Union(IgnoredPropertyNames).ToList();
        }
    }
}
