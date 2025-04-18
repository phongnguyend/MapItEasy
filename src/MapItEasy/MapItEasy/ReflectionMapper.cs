using System.Linq.Expressions;
using System.Reflection;

namespace MapItEasy;

public class ReflectionMapper : IMapper
{
    static readonly object _lock = new();

    private static readonly Dictionary<(Type From, Type To), List<(string Name, MethodInfo Get, MethodInfo Set)>> _cache = [];

    private static List<(string Name, MethodInfo Get, MethodInfo Set)> GetOrAdd((Type From, Type To) key)
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

            var fromProps = key.From.GetProperties();
            var toProps = key.To.GetProperties();

            List<(string Name, MethodInfo, MethodInfo)> entry = new();
            foreach (var from in fromProps)
            {
                var to = toProps.FirstOrDefault(x => x.Name == from.Name);
                if (to == null)
                {
                    continue;
                }

                if (!from.IsAssignableTo(to))
                {
                    continue;
                }

                if (from.GetMethod == null || to.SetMethod == null)
                {
                    continue;
                }

                entry.Add((from.Name, from.GetMethod, to.SetMethod));
            }

            _cache[key] = entry;
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
        foreach (var (Name, Get, Set) in entry)
        {
            var val = Get.Invoke(source, null);
            Set.Invoke(target, [val]);
        }
    }

    public TTarget MapProperties<TSource, TTarget>(TSource source, Expression<Func<TSource, object>> propertiesSelector) where TTarget : class, new()
    {
        var result = new TTarget();
        MapProperties(source, result, propertiesSelector);
        return result;
    }

    public void MapProperties<TSource, TTarget>(TSource source, TTarget target, Expression<Func<TSource, object>> propertiesSelector) where TTarget : class
    {
        MapProperties(source, target, propertiesSelector.Body.GetMemberNames().ToArray());
    }

    public TTarget MapExclude<TSource, TTarget>(TSource source, Expression<Func<TSource, object>> propertiesSelector) where TTarget : class, new()
    {
        var result = new TTarget();
        MapExclude(source, result, propertiesSelector);
        return result;
    }

    public void MapExclude<TSource, TTarget>(TSource source, TTarget target, Expression<Func<TSource, object>> propertiesSelector) where TTarget : class
    {
        MapWithoutProperties(source, target, propertiesSelector.Body.GetMemberNames().ToArray());
    }

    private void MapProperties<TSource, TTarget>(TSource source, TTarget target, string[] properties) where TTarget : class
    {
        if (properties == null || properties.Length == 0)
        {
            return;
        }

        var key = (from: typeof(TSource), to: typeof(TTarget));

        var entry = GetOrAdd(key);
        foreach (var (Name, Get, Set) in entry)
        {
            if (!properties.Contains(Name))
            {
                continue;
            }

            var val = Get.Invoke(source, null);
            Set.Invoke(target, [val]);
        }
    }

    private void MapWithoutProperties<TSource, TTarget>(TSource source, TTarget target, string[] properties) where TTarget : class
    {
        var key = (from: typeof(TSource), to: typeof(TTarget));

        var entry = GetOrAdd(key);
        foreach (var (Name, Get, Set) in entry)
        {
            if (properties != null && properties.Contains(Name))
            {
                continue;
            }

            var val = Get.Invoke(source, null);
            Set.Invoke(target, [val]);
        }
    }
}
