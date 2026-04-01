using System.Collections.Concurrent;
using System.Reflection;

namespace MapItEasy;

public class ReflectionMapper : IMapper
{
    private static readonly ReflectionMapper _instance = new();

    public static ReflectionMapper Instance => _instance;

    private static readonly ConcurrentDictionary<(Type From, Type To), List<(string Name, MethodInfo Get, MethodInfo Set)>> _cache = [];

    private static List<(string Name, MethodInfo Get, MethodInfo Set)> GetOrAdd((Type From, Type To) key)
    {
        return _cache.GetOrAdd(key, (key) =>
        {
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

            return entry;
        });
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
                MapInclude(source, target, options.IncludeProperties.Body.GetMemberNames().ToArray());
                return;
            }

            if (options.ExcludeProperties != null)
            {
                MapExclude(source, target, options.ExcludeProperties.Body.GetMemberNames().ToArray());
                return;
            }
        }

        var key = (from: typeof(TSource), to: typeof(TTarget));

        var entry = GetOrAdd(key);
        foreach (var (Name, Get, Set) in entry)
        {
            var val = Get.Invoke(source, null);
            Set.Invoke(target, [val]);
        }
    }

    private void MapInclude<TSource, TTarget>(TSource source, TTarget target, string[] properties) where TTarget : class
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

    private void MapExclude<TSource, TTarget>(TSource source, TTarget target, string[] properties) where TTarget : class
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
