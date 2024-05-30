namespace DeepEqual.Formatting;

internal static class FormatterHelper
{
    internal static string Prettify(object? value)
    {
        if (value == null)
            return "(null)";

        if (value is string)
            return $"\"{value}\"";

        return value.ToString() ?? string.Empty;
    }
}
