using BenchmarkDotNet.Attributes;

namespace MapItEasy.Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    private static readonly A _source = new A { Id = 1, Name = "abc1", Description = "xyz1" };

    private const int ITERATIONS = 1_000_000;


    [Benchmark]
    public void ExpressionMapper_Map()
    {
        var target = new B();

        for (var i = 0; i < ITERATIONS; i++)
        {
            ExpressionMapper.Instance.Map(_source, target);
        }
    }

    [Benchmark]
    public void ReflectionMapper_Map()
    {
        var target = new B();

        for (var i = 0; i < ITERATIONS; i++)
        {
            ReflectionMapper.Instance.Map(_source, target);
        }
    }

    [Benchmark]
    public void GeneratedMapper_Map()
    {
        var target = new B();

        for (var i = 0; i < ITERATIONS; i++)
        {
            _source.Map2(target);
        }
    }

    [Benchmark]
    public void ManualMap()
    {
        var target = new B();

        for (var i = 0; i < ITERATIONS; i++)
        {
            target.Id = _source.Id;
            target.Name = _source.Name;
            target.Description = _source.Description;
            target.NotNullToNull = _source.NotNullToNull;

            if (_source.NullToNotNull != null)
                target.NullToNotNull = _source.NullToNotNull.Value;
        }
    }
}

public class A
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Age { get; set; }

    public int NotNullToNull { get; set; }

    public int? NullToNotNull { get; set; }
}

public class B
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Age { get; set; }

    public int? NotNullToNull { get; set; }

    public int NullToNotNull { get; set; }
}

public static partial class AExtensions
{
    [GeneratedMapping]
    public static partial B Map1(A source);

    [GeneratedMapping]
    public static partial B Map2(this A source);

    [GeneratedMapping]
    public static partial B Map3(A source, MappingOptions<A>? options = null);

    [GeneratedMapping]
    public static partial B Map4(this A source, MappingOptions<A>? options = null);

    [GeneratedMapping]
    public static partial void Map1(A source, B target);

    [GeneratedMapping]
    public static partial void Map2(this A source, B target);

    [GeneratedMapping]
    public static partial void Map3(A source, B target, MappingOptions<A>? options = null);

    [GeneratedMapping]
    public static partial void Map4(this A source, B target, MappingOptions<A>? options = null);
}