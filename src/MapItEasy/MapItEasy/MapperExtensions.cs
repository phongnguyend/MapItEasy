namespace MapItEasy;

public static class MapperExtensions
{
    private static readonly IMapper _mapper = new ExpressionMapper();

    public static TTarget Map<TSource, TTarget>(this TSource source) where TTarget : class, new()
    {
        return _mapper.Map<TSource, TTarget>(source);
    }

    public static void Map<TSource, TTarget>(this TSource source, TTarget target) where TTarget : class
    {
        _mapper.Map(source, target);
    }
}
