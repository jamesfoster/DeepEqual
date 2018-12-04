//ncrunch: no coverage start

namespace DeepEqual.Syntax
{
	using System;
	using System.Runtime.Serialization;

	using DeepEqual.Formatting;

	public class DeepEqualException : Exception
	{
		public IComparisonContext Context { get; set; }

		public DeepEqualException(IComparisonContext context)
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