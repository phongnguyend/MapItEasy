using System.Reflection;

namespace MapItEasy;

public static class TypeExtenstions
{
    public static bool IsNullable(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    public static bool IsAssignableTo(this PropertyInfo from, PropertyInfo to)
    {
        var fromType = Nullable.GetUnderlyingType(from.PropertyType) ?? from.PropertyType;
        var toType = Nullable.GetUnderlyingType(to.PropertyType) ?? to.PropertyType;

        return fromType == toType;
    }
}
