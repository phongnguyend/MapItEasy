namespace MapItEasy.Tests;

public class GeneratedMappingExtensionMethodsTests : BaseMapperTests
{
    public GeneratedMappingExtensionMethodsTests() : base(new GeneratedMappingExtensionMethodsMapper())
    {
    }
}

public class GeneratedMappingExtensionMethodsMapper : IMapper
{
    public TTarget Map<TSource, TTarget>(TSource source, MappingOptions<TSource>? options = null) where TTarget : class, new()
    {
        var target = new TTarget();
        Map(source, target, options);
        return target;
    }

    public void Map<TSource, TTarget>(TSource source, TTarget target, MappingOptions<TSource>? options = null) where TTarget : class
    {
        if (source is BaseMapperTests.A a && target is BaseMapperTests.B b)
        {
            var typedOptions = options as MappingOptions<BaseMapperTests.A>;
            a.MapAToB(b, typedOptions);
        }
        else if (source is BaseMapperTests.B b2 && target is BaseMapperTests.A a2)
        {
            var typedOptions = options as MappingOptions<BaseMapperTests.B>;
            b2.MapBToA(a2, typedOptions);
        }
        else
        {
            throw new NotSupportedException($"Mapping from {typeof(TSource)} to {typeof(TTarget)} is not supported.");
        }
    }
}

public static partial class GeneratedMappingExtensionMethods
{
    [GeneratedMapping]
    public static partial void MapAToB(this BaseMapperTests.A source, BaseMapperTests.B target, MappingOptions<BaseMapperTests.A>? options = null);

    [GeneratedMapping]
    public static partial void MapBToA(this BaseMapperTests.B source, BaseMapperTests.A target, MappingOptions<BaseMapperTests.B>? options = null);
}
