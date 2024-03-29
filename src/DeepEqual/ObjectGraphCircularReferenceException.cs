﻿using System.Runtime.Serialization;

namespace DeepEqual;

[Serializable]
public class ObjectGraphCircularReferenceException : Exception
{
	public string Breadcrumb { get; set; }
	public object Value1 { get; set; }
	public object Value2 { get; set; }

	public ObjectGraphCircularReferenceException(string message) : base(message)
	{
	}

	public ObjectGraphCircularReferenceException(string message, string breadcrumb, object value1, object value2)
		: base(message)
	{
		Breadcrumb = breadcrumb;
		Value1 = value1;
		Value2 = value2;
	}

	public ObjectGraphCircularReferenceException(string message, Exception innerException) : base(message, innerException)
	{
	}

	protected ObjectGraphCircularReferenceException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}