namespace DeepEqual;

[Serializable]
public class ObjectGraphCircularReferenceException : Exception
{
    public BreadcrumbPair Breadcrumb { get; }
    public object? Left { get; }
    public object? Right { get; }

    public ObjectGraphCircularReferenceException(
        string message,
        BreadcrumbPair breadcrumb,
        object? left,
        object? right
    )
        : base(message)
    {
        Breadcrumb = breadcrumb;
        Left = left;
        Right = right;
    }
}
