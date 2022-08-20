namespace DeepEqual;

public abstract class Difference
{
	protected Difference(string breadcrumb)
	{
		Breadcrumb = breadcrumb;
	}

	public string Breadcrumb { get; }
}