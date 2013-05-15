namespace DeepEquals
{
	public class Difference
	{
		public string Breadcrumb { get; set; }
		public string ChildProperty { get; set; }
		public object Value1 { get; set; }
		public object Value2 { get; set; }

		protected bool Equals(Difference other)
		{
			return string.Equals(Breadcrumb, other.Breadcrumb)
			       && string.Equals(ChildProperty, other.ChildProperty)
			       && Equals(Value1, other.Value1)
			       && Equals(Value2, other.Value2);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Difference) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Breadcrumb != null ? Breadcrumb.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (ChildProperty != null ? ChildProperty.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (Value1 != null ? Value1.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (Value2 != null ? Value2.GetHashCode() : 0);
				return hashCode;
			}
		}

		public override string ToString()
		{
			var format = "Actual.{0}.{1} != Expected.{0}.{1} (Actual: {2}, Expected: {3})";

			if (ChildProperty == null)
				format = "Actual.{0} != Expected.{0} (Actual: {2}, Expected: {3})";

			return string.Format(format, Breadcrumb, ChildProperty, Value1, Value2);
		}
	}
}