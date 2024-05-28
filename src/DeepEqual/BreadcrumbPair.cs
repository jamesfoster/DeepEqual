namespace DeepEqual;

public record BreadcrumbPair(string Left, string Right)
{
    public static readonly BreadcrumbPair Empty = new(string.Empty, string.Empty);

    public BreadcrumbPair(string path)
        : this(path, path) { }

    public BreadcrumbPair Dot(string property)
    {
        return new BreadcrumbPair($"{Left}.{property}", $"{Right}.{property}");
    }

    public BreadcrumbPair Index(string index)
    {
        return new BreadcrumbPair($"{Left}[{index}]", $"{Right}[{index}]");
    }
}
