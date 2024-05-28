namespace DeepEqual.Formatting;

public interface IDifferenceFormatterFactory
{
    IDifferenceFormatter GetFormatter(Difference difference);
}
