using System.Runtime.Serialization;

namespace DeepEqual;

[Serializable]
public class ObjectGraphCircularReferenceException : Exception
{
    public BreadcrumbPair Breadcrumb { get; set; }
    public object Value1 { get; set; }
    public object Value2 { get; set; }

    public ObjectGraphCircularReferenceException(
        string message,
        BreadcrumbPair breadcrumb,
        object value1,
        object value2
    )
        : base(message)
    {
        Breadcrumb = breadcrumb;
        Value1 = value1;
        Value2 = value2;
    }
}
