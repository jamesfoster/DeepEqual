namespace DeepEqual;

[Serializable]
public class ExpectedMissingProperty : Exception
{
    public ExpectedMissingProperty(string message)
        : base(message) { }
}
