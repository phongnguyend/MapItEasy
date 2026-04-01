namespace MapItEasy;

public interface IMapper
{
    public TTarget Map<TSource, TTarget>(TSource source, MappingOptions<TSource>? options = null) where TTarget : class, new();

    public void Map<TSource, TTarget>(TSource source, TTarget target, MappingOptions<TSource>? options = null) where TTarget : class;
}
