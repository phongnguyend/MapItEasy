using System.Linq.Expressions;

namespace MapItEasy;

public class MappingOptions<TSource>
{
    public Expression<Func<TSource, object>>? IncludeProperties { get; set; }

    public Expression<Func<TSource, object>>? ExcludeProperties { get; set; }

    public void Validate()
    {
        if (IncludeProperties != null && ExcludeProperties != null)
        {
            throw new InvalidOperationException("Cannot specify both IncludeProperties and ExcludeProperties.");
        }
    }
}
