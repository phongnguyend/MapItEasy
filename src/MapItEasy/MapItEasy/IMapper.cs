namespace MapItEasy;

public interface IMapper
{
    public TTarget Map<TSource, TTarget>(TSource source) where TTarget : class, new();

    public void Map<TSource, TTarget>(TSource source, TTarget target) where TTarget : class;
}
