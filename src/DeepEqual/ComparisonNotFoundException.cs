namespace DeepEqual;

public class ComparisonNotFoundException : Exception
{
    public ComparisonNotFoundException(string message) : base(message)
    {
    }
}