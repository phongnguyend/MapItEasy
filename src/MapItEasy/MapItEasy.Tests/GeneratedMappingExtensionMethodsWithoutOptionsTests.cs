namespace MapItEasy.Tests;

public class GeneratedMappingExtensionMethodsWithoutOptionsTests : BaseMapperTests
{
    public GeneratedMappingExtensionMethodsWithoutOptionsTests() : base(new GeneratedMappingExtensionMethodsWithoutOptionsMapper(), ignoreOptions: true)
    {
    }
}

public class GeneratedMappingExtensionMethodsWithoutOptionsMapper : IMapper
{
    public TTarget Map<TSource, TTarget>(TSource source, MappingOptions<TSource>? options = null) where TTarget : class, new()
    {
        if (source is BaseMapperTests.A a)
        {
            return (TTarget)(object)a.MapToB();
        }
        else if (source is BaseMapperTests.B b2)
        {
            return (TTarget)(object)b2.MapToA();
        }
        else
        {
            throw new NotSupportedException($"Mapping from {typeof(TSource)} to {typeof(TTarget)} is not supported.");
        }
    }

    public void Map<TSource, TTarget>(TSource source, TTarget target, MappingOptions<TSource>? options = null) where TTarget : class
    {
        if (source is BaseMapperTests.A a && target is BaseMapperTests.B b)
        {
            a.MapAToB(b);
        }
        else if (source is BaseMapperTests.B b2 && target is BaseMapperTests.A a2)
        {
            b2.MapBToA(a2);
        }
        else
        {
            throw new NotSupportedException($"Mapping from {typeof(TSource)} to {typeof(TTarget)} is not supported.");
        }
    }
}

public static partial class GeneratedMappingExtensionMethodsWithoutOptions
{
    [GeneratedMapping]
    public static partial void MapAToB(this BaseMapperTests.A source, BaseMapperTests.B target);

    [GeneratedMapping]
    public static partial void MapBToA(this BaseMapperTests.B source, BaseMapperTests.A target);

    [GeneratedMapping]
    public static partial BaseMapperTests.B MapToB(this BaseMapperTests.A source);

    [GeneratedMapping]
    public static partial BaseMapperTests.A MapToA(this BaseMapperTests.B source);
}
