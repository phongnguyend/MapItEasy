namespace MapItEasy.Tests;

public abstract class BaseMapperTests
{
    IMapper _mapper;

    public BaseMapperTests(IMapper mapper)
    {
        _mapper = mapper;
    }

    [Fact]
    public void ReturnNewObject()
    {
        var source = new A { Id = 1, Name = "abc1", Description = "xyz1" };

        var target = _mapper.Map<A, B>(source);

        Assert.Equal(1, target.Id);
        Assert.Equal("abc1", target.Name);
        Assert.Equal("xyz1", target.Description);
    }

    [Fact]
    public void MapExistingObject()
    {
        var source = new A { Id = 1, Name = "abc1", Description = "xyz1" };
        var target = new B();

        _mapper.Map(source, target);

        Assert.Equal(1, target.Id);
        Assert.Equal("abc1", target.Name);
        Assert.Equal("xyz1", target.Description);
    }

    public class A
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Age { get; set; }

        public int NotNullToNull { get; set; }

        public int? NullToNotNull { get; set; }
    }

    public class B
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Age { get; set; }

        public int? NotNullToNull { get; set; }

        public int NullToNotNull { get; set; }
    }
}