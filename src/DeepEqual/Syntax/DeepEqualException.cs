namespace DeepEqual.Syntax
{
	using System;
	using System.Runtime.Serialization;

	public class DeepEqualException : Exception
	{
		public ComparisonContext Context { get; set; }

		public DeepEqualException(ComparisonContext context)
			: this(new DeepEqualExceptionMessageBuilder(context).GetMessage())
		{
			Context = context;
		}

		public DeepEqualException(string message)
			: base(message)
		{
		}

		public DeepEqualException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public DeepEqualException(SerializationInfo info, StreamingContext context)
			:base(info, context)
		{
		}
	}
}