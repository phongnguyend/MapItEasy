using System.Linq.Expressions;

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

            List<Expression> assigns = [];
            foreach (var fromProp in key.From.GetProperties())
            {
                var toProp = key.To.GetProperty(fromProp.Name);
                if (toProp == null)
                {
                    continue;
                }

                if (!fromProp.IsAssignableTo(toProp))
                {
                    continue;
                }

                if (fromProp.PropertyType.IsNullable() && !toProp.PropertyType.IsNullable())
                {
                    // from.Property != null
                    var notNullCheck = Expression.NotEqual(Expression.MakeMemberAccess(fromParam, fromProp), Expression.Constant(null));

                    // to.Property
                    Expression left = Expression.MakeMemberAccess(toParam, toProp);

                    // from.Property
                    Expression right = Expression.MakeMemberAccess(fromParam, fromProp);
                    right = Expression.Convert(right, toProp.PropertyType);

                    // to.Property = from.Property;
                    var assign = Expression.Assign(left, right);

                    // if (from.Property != null) { to.Property = from.Property; }
                    var ifBlock = Expression.IfThen(notNullCheck, assign);

                    assigns.Add(ifBlock);
                }
                else
                {
                    // to.Property
                    Expression left = Expression.MakeMemberAccess(toParam, toProp);

                    // from.Property
                    Expression right = Expression.MakeMemberAccess(fromParam, fromProp);

                    if (toProp.PropertyType.IsNullable() && !fromProp.PropertyType.IsNullable())
                    {
                        right = Expression.Convert(right, toProp.PropertyType);
                    }

                    // to.Property = from.Property;
                    var assign = Expression.Assign(left, right);

                    assigns.Add(assign);
                }
            }

            var body = Expression.Block(assigns);

            var fucn = Expression.Lambda(body, false, fromParam, toParam).Compile();

            _cache[key] = fucn;
        }

        return _cache[key];
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
