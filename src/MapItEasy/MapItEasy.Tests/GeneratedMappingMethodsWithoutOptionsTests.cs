namespace MapItEasy.Tests;

public class GeneratedMappingMethodsWithoutOptionsTests : BaseMapperTests
{
    public GeneratedMappingMethodsWithoutOptionsTests() : base(new GeneratedMappingMethodsWithoutOptionsMapper(), ignoreOptions: true)
    {
    }
}

public class GeneratedMappingMethodsWithoutOptionsMapper : IMapper
{
    public TTarget Map<TSource, TTarget>(TSource source, MappingOptions<TSource>? options = null) where TTarget : class, new()
    {
        if (source is BaseMapperTests.A a)
        {
            return (TTarget)(object)GeneratedMappingMethodsWithoutOptions.MapToB(a);
        }
        else if (source is BaseMapperTests.B b2)
        {
            return (TTarget)(object)GeneratedMappingMethodsWithoutOptions.MapToA(b2);
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
            GeneratedMappingMethodsWithoutOptions.MapAToB(a, b);
        }
        else if (source is BaseMapperTests.B b2 && target is BaseMapperTests.A a2)
        {
            GeneratedMappingMethodsWithoutOptions.MapBToA(b2, a2);
        }
        else
        {
            throw new NotSupportedException($"Mapping from {typeof(TSource)} to {typeof(TTarget)} is not supported.");
        }
    }
}

public static partial class GeneratedMappingMethodsWithoutOptions
{
    [GeneratedMapping]
    public static partial void MapAToB(BaseMapperTests.A source, BaseMapperTests.B target);

    [GeneratedMapping]
    public static partial void MapBToA(BaseMapperTests.B source, BaseMapperTests.A target);

    [GeneratedMapping]
    public static partial BaseMapperTests.B MapToB(BaseMapperTests.A source);

    [GeneratedMapping]
    public static partial BaseMapperTests.A MapToA(BaseMapperTests.B source);
}
