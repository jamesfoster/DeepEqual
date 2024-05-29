using System.Diagnostics.CodeAnalysis;

namespace DeepEqual;

public class ComplexObjectComparison : IComparison
{
    public IComparison Inner { get; set; }

    public bool IgnoreUnmatchedProperties { get; set; }

    public List<Func<PropertyPair, bool>> IgnoredProperties { get; set; }
    public List<Func<Type, Type, string, string?>> MappedProperties { get; set; }

    public ComplexObjectComparison(IComparison inner)
    {
        Inner = inner;
        IgnoredProperties = [];
        MappedProperties = [];
    }

    public bool CanCompare(Type leftType, Type rightType)
    {
        return (leftType.IsClass && rightType.IsClass)
            || ReflectionCache.IsValueTypeWithReferenceFields(leftType)
            || ReflectionCache.IsValueTypeWithReferenceFields(rightType);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        var comparer = new ComplexObjectComparer(
            Inner,
            IgnoreUnmatchedProperties,
            IgnoredProperties,
            MappedProperties
        );

        return comparer.CompareObjects(context, leftValue, rightValue);
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
        if (!GetMemberName(property, out var name))
        {
            throw new ArgumentException($"Expression must be a simple member access: {property}");
        }
        IgnoreProperty(typeof(T), name);
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
        {
            static bool Matches(PropertyReader? reader, Type type, string name)
            {
                return reader is not null
                    && type.IsAssignableFrom(reader.DeclaringType)
                    && reader.Name == name;
            }
            return Matches(property.Left, type, propertyName)
                || Matches(property.Right, type, propertyName);
        });
    }

    public void IgnoreProperty(Func<PropertyPair, bool> item)
    {
        IgnoredProperties.Add(item);
    }
}
