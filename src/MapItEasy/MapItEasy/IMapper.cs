using System.Linq.Expressions;

namespace MapItEasy;

public interface IMapper
{
    public TTarget Map<TSource, TTarget>(TSource source) where TTarget : class, new();

    public TTarget MapProperties<TSource, TTarget>(TSource source, Expression<Func<TSource, object>> propertiesSelector) where TTarget : class, new();

    public TTarget MapExclude<TSource, TTarget>(TSource source, Expression<Func<TSource, object>> propertiesSelector) where TTarget : class, new();

    public void Map<TSource, TTarget>(TSource source, TTarget target) where TTarget : class;

    public void MapProperties<TSource, TTarget>(TSource source, TTarget target, Expression<Func<TSource, object>> propertiesSelector) where TTarget : class;
    
    public void MapExclude<TSource, TTarget>(TSource source, TTarget target, Expression<Func<TSource, object>> propertiesSelector) where TTarget : class;
}
