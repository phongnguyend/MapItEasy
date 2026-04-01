using System.Linq.Expressions;

namespace MapItEasy;

public class MappingOptions<TSource>
{
    public Expression<Func<TSource, object>>? Include
    {
        set
        {
            IncludeProperties = value?.Body.GetMemberNames();
        }
    }

    public Expression<Func<TSource, object>>? Exclude
    {
        set
        {
            ExcludeProperties = value?.Body.GetMemberNames();
        }
    }

    public IReadOnlyCollection<string>? IncludeProperties { get; set; }

    public IReadOnlyCollection<string>? ExcludeProperties { get; set; }

    public void Validate()
    {
        if (IncludeProperties != null && ExcludeProperties != null)
        {
            throw new InvalidOperationException("Cannot specify both IncludeProperties and ExcludeProperties.");
        }
    }
}
