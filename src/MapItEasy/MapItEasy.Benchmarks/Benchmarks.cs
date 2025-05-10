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
    public void ManualMap()
    {
        var target = new B();

        for (var i = 0; i < ITERATIONS; i++)
        {
            target.Id = _source.Id;
            target.Name = _source.Name;
            target.Description = _source.Description;
            target.NotNullToNull = _source.NotNullToNull;
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
