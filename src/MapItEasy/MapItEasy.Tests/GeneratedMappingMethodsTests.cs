namespace MapItEasy.Tests;

public class GeneratedMappingMethodsTests : BaseMapperTests
{
    public GeneratedMappingMethodsTests() : base(new GeneratedMappingMethodsMapper())
    {
    }
}

public class GeneratedMappingMethodsMapper : IMapper
{
    public TTarget Map<TSource, TTarget>(TSource source, MappingOptions<TSource>? options = null) where TTarget : class, new()
    {
        if (source is BaseMapperTests.A a)
        {
            var typedOptions = options as MappingOptions<BaseMapperTests.A>;
            return (TTarget)(object)GeneratedMappingMethods.MapToB(a, typedOptions);
        }
        else if (source is BaseMapperTests.B b2)
        {
            var typedOptions = options as MappingOptions<BaseMapperTests.B>;
            return (TTarget)(object)GeneratedMappingMethods.MapToA(b2, typedOptions);
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
            var typedOptions = options as MappingOptions<BaseMapperTests.A>;
            GeneratedMappingMethods.MapAToB(a, b, typedOptions);
        }
        else if (source is BaseMapperTests.B b2 && target is BaseMapperTests.A a2)
        {
            var typedOptions = options as MappingOptions<BaseMapperTests.B>;
            GeneratedMappingMethods.MapBToA(b2, a2, typedOptions);
        }
        else
        {
            throw new NotSupportedException($"Mapping from {typeof(TSource)} to {typeof(TTarget)} is not supported.");
        }
    }
}

public static partial class GeneratedMappingMethods
{
    [GeneratedMapping]
    public static partial void MapAToB(BaseMapperTests.A source, BaseMapperTests.B target, MappingOptions<BaseMapperTests.A>? options = null);

    [GeneratedMapping]
    public static partial void MapBToA(BaseMapperTests.B source, BaseMapperTests.A target, MappingOptions<BaseMapperTests.B>? options = null);

    [GeneratedMapping]
    public static partial BaseMapperTests.B MapToB(BaseMapperTests.A source, MappingOptions<BaseMapperTests.A>? options = null);

    [GeneratedMapping]
    public static partial BaseMapperTests.A MapToA(BaseMapperTests.B source, MappingOptions<BaseMapperTests.B>? options = null);
}
