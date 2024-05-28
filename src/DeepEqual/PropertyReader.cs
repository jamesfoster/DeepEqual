namespace DeepEqual;

public record PropertyReader(string Name, Func<object, object?> Read, Type DeclaringType);