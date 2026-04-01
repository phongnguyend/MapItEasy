using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace MapItEasy;

public class ExpressionMapper : IMapper
{
    private static readonly ExpressionMapper _instance = new();

    public static ExpressionMapper Instance => _instance;

    private static readonly ConcurrentDictionary<(Type From, Type To, MapType MapType), Delegate> _cache = [];

    private static Delegate GetOrAdd((Type From, Type To, MapType MapType) key)
    {
        return _cache.GetOrAdd(key, (key) =>
        {
            var fromParam = Expression.Parameter(key.From);
            var toParam = Expression.Parameter(key.To);
            var propertiesParam = Expression.Parameter(typeof(IReadOnlyCollection<string>));

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

                    if (key.MapType == MapType.SelectedProperties)
                    {
                        // if(properties.Contains(propertyName))
                        assigns.Add(CreateContainsCheckExpression(fromProp.Name, propertiesParam, ifBlock));
                    }
                    else if (key.MapType == MapType.ExcludedProperties)
                    {
                        // if(!properties.Contains(propertyName))
                        assigns.Add(CreateNotContainsCheckExpression(fromProp.Name, propertiesParam, ifBlock));
                    }
                    else
                    {
                        assigns.Add(ifBlock);
                    }
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

                    if (key.MapType == MapType.SelectedProperties)
                    {
                        // if(properties.Contains(propertyName))
                        assigns.Add(CreateContainsCheckExpression(fromProp.Name, propertiesParam, assign));
                    }
                    else if (key.MapType == MapType.ExcludedProperties)
                    {
                        // if(!properties.Contains(propertyName))
                        assigns.Add(CreateNotContainsCheckExpression(fromProp.Name, propertiesParam, assign));
                    }
                    else
                    {
                        assigns.Add(assign);
                    }
                }
            }

            var body = Expression.Block(assigns);

            var fucn = key.MapType == MapType.AllProperties ?
                Expression.Lambda(body, false, fromParam, toParam).Compile() :
                Expression.Lambda(body, false, fromParam, toParam, propertiesParam).Compile();

            return fucn;
        });
    }

    private static Expression CreateContainsCheckExpression(string propertyName, ParameterExpression propertiesParam, Expression expression)
    {
        MethodInfo containsMethod = GetContainsMethod();

        // properties.Contains(propertyName)
        var containsCall = Expression.Call(containsMethod, propertiesParam, Expression.Constant(propertyName));

        // if(properties.Contains(propertyName))
        var ifBlock = Expression.IfThen(containsCall, expression);

        return ifBlock;
    }

    private static Expression CreateNotContainsCheckExpression(string propertyName, ParameterExpression propertiesParam, Expression expression)
    {
        MethodInfo containsMethod = GetContainsMethod();

        // properties.Contains(propertyName)
        var containsCall = Expression.Call(containsMethod, propertiesParam, Expression.Constant(propertyName));

        // !properties.Contains(propertyName)
        var notContains = Expression.Not(containsCall);

        // if(!properties.Contains(propertyName))
        var ifBlock = Expression.IfThen(notContains, expression);

        return ifBlock;
    }

    private static MethodInfo GetContainsMethod()
    {
        return typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(string));
    }

    public TTarget Map<TSource, TTarget>(TSource source, MappingOptions<TSource>? options = null) where TTarget : class, new()
    {
        var result = new TTarget();
        Map(source, result, options);
        return result;
    }

    public void Map<TSource, TTarget>(TSource source, TTarget target, MappingOptions<TSource>? options = null) where TTarget : class
    {
        if (options != null)
        {
            options.Validate();

            if (options.IncludeProperties != null)
            {
                if (options.IncludeProperties.Count == 0)
                {
                    return;
                }

                var key = (from: typeof(TSource), to: typeof(TTarget), MapType.SelectedProperties);
                var entry = GetOrAdd(key);
                entry.DynamicInvoke(source, target, options.IncludeProperties);
                return;
            }
            
            if (options.ExcludeProperties != null)
            {
                if (options.ExcludeProperties.Count > 0)
                {
                    var key = (from: typeof(TSource), to: typeof(TTarget), MapType.ExcludedProperties);
                    var entry = GetOrAdd(key);
                    entry.DynamicInvoke(source, target, options.ExcludeProperties);
                    return;
                }
            }
        }

        var allKey = (from: typeof(TSource), to: typeof(TTarget), MapType.AllProperties);
        var allEntry = GetOrAdd(allKey);
        allEntry.DynamicInvoke(source, target);
    }

    private enum MapType
    {
        AllProperties,
        SelectedProperties,
        ExcludedProperties
    }
}
