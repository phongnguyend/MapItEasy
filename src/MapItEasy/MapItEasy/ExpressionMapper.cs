using System.Linq.Expressions;
using System.Reflection;

namespace MapItEasy;

public class ExpressionMapper : IMapper
{
    static readonly object _lock = new();

    private static readonly Dictionary<(Type From, Type To), Delegate> _cache = [];

    private static Delegate GetOrAdd((Type From, Type To) key)
    {
        if (_cache.ContainsKey(key))
        {
            return _cache[key];
        }

        lock (_lock)
        {
            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }

            var fromParam = Expression.Parameter(key.From);
            var toParam = Expression.Parameter(key.To);

            List<BinaryExpression> assigns = [];
            foreach (var fromProp in key.From.GetProperties())
            {
                var toProp = key.To.GetProperty(fromProp.Name);
                if (toProp == null)
                {
                    continue;
                }

                if (!IsAssignable(fromProp, toProp))
                {
                    continue;
                }

                Expression left = Expression.MakeMemberAccess(toParam, toProp);
                Expression right = Expression.MakeMemberAccess(fromParam, fromProp);

                if (IsNullable(toProp.PropertyType) && !IsNullable(fromProp.PropertyType))
                {
                    right = Expression.Convert(right, toProp.PropertyType);
                }

                var assign = Expression.Assign(left, right);
                assigns.Add(assign);
            }

            var body = Expression.Block(assigns);

            var fucn = Expression.Lambda(body, false, fromParam, toParam).Compile();

            _cache[key] = fucn;
        }

        return _cache[key];
    }

    private static bool IsAssignable(PropertyInfo from, PropertyInfo to)
    {
        var fromType = Nullable.GetUnderlyingType(from.PropertyType) ?? from.PropertyType;
        var toType = Nullable.GetUnderlyingType(to.PropertyType) ?? to.PropertyType;

        if (fromType != toType)
        {
            return false;
        }

        if (IsNullable(from.PropertyType) && !IsNullable(to.PropertyType))
        {
            return false;
        }

        return true;
    }

    private static bool IsNullable(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    public TTarget Map<TSource, TTarget>(TSource source) where TTarget : class, new()
    {
        var result = new TTarget();
        Map(source, result);
        return result;
    }

    public void Map<TSource, TTarget>(TSource source, TTarget target) where TTarget : class
    {
        var key = (from: typeof(TSource), to: typeof(TTarget));

        var entry = GetOrAdd(key);
        entry.DynamicInvoke(source, target);
    }
}
