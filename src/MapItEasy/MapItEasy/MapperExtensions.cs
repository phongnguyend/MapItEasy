namespace MapItEasy;

public static class MapperExtensions
{
    private static readonly IMapper _mapper = ExpressionMapper.Instance;

    public static TTarget Map<TSource, TTarget>(this TSource source, MappingOptions<TSource>? options = null) where TTarget : class, new()
    {
        return _mapper.Map<TSource, TTarget>(source, options);
    }

    public static void Map<TSource, TTarget>(this TSource source, TTarget target, MappingOptions<TSource>? options = null) where TTarget : class
    {
        _mapper.Map(source, target, options);
    }
}
