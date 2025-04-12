using System.Reflection;

namespace MapItEasy;

public class ReflectionMapper : IMapper
{
    static readonly object _lock = new();

    private static readonly Dictionary<(Type From, Type To), List<(MethodInfo Get, MethodInfo Set)>> _cache = [];

    private static List<(MethodInfo Get, MethodInfo Set)> GetOrAdd((Type From, Type To) key)
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

            List<(MethodInfo, MethodInfo)> entry = new();
            foreach (var from in fromProps)
            {
                var to = toProps.FirstOrDefault(x => x.Name == from.Name);
                if (to == null)
                {
                    continue;
                }

                entry.Add((from.GetMethod, to.SetMethod));
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
        foreach (var (Get, Set) in entry)
        {
            var val = Get.Invoke(source, null);
            Set.Invoke(target, [val]);
        }
    }
}
