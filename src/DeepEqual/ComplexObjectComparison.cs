using System.Diagnostics.CodeAnalysis;

namespace DeepEqual;

public class ComplexObjectComparison : IComparison
{
    public IComparison Inner { get; set; }

    public bool IgnoreUnmatchedProperties { get; set; }

    public List<Func<PropertyReader, bool>> IgnoredProperties { get; set; }
    public List<Func<Type, Type, string, string?>> MappedProperties { get; set; }

    public ComplexObjectComparison(IComparison inner)
    {
        Inner = inner;
        IgnoredProperties = [];
        MappedProperties = [];
    }

    public bool CanCompare(Type type1, Type type2)
    {
        return (type1.IsClass && type2.IsClass)
            || ReflectionCache.IsValueTypeWithReferenceFields(type1)
            || ReflectionCache.IsValueTypeWithReferenceFields(type2);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? value1,
        object? value2
    )
    {
        var comparer = new ComplexObjectComparer(
            Inner,
            IgnoreUnmatchedProperties,
            IgnoredProperties,
            MappedProperties
        );

        return comparer.CompareObjects(context, value1, value2);
    }

    public void MapProperty<A, B>(
        Expression<Func<A, object?>> left,
        Expression<Func<B, object?>> right
    )
    {
        if (GetMemberName(left, out var leftName) && GetMemberName(right, out var rightName))
        {
            MappedProperties.Add(
                (leftType, rightType, propName) =>
                {
                    if (leftType == typeof(A) && rightType == typeof(B) && propName == leftName)
                    {
                        return rightName;
                    }
                    return null;
                }
            );
        }
    }

    public void IgnoreProperty<T>(Expression<Func<T, object?>> property)
    {
        if (GetMemberName(property, out var name))
        {
            IgnoreProperty(typeof(T), name);
        }
    }

    private static bool GetMemberName<T>(Expression<Func<T, object?>> property, out string? name)
    {
        var exp = property.Body;

        if (exp is UnaryExpression cast)
        {
            exp = cast.Operand; // implicit cast to object
        }

        if (exp is MemberExpression member)
        {
            name = member.Member.Name;
            return true;
        }

        name = null;
        return false;
    }

    private void IgnoreProperty(Type type, string? propertyName)
    {
        if (propertyName is null)
            return;

        IgnoreProperty(property =>
            type.IsAssignableFrom(property.DeclaringType) && property.Name == propertyName
        );
    }

    public void IgnoreProperty(Func<PropertyReader, bool> item)
    {
        IgnoredProperties.Add(item);
    }
}
