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

    [Fact]
    public void IfTypesUnmatched_Should_Ignore_1()
    {
        var source = new A { Age = "1" };
        var target = new B();

        _mapper.Map(source, target);

        Assert.Equal(0, target.Age);
    }

    [Fact]
    public void IfTypesUnmatched_Should_Ignore_2()
    {
        var source = new B { Age = 1 };
        var target = new A();

        _mapper.Map(source, target);

        Assert.Null(target.Age);
    }

    [Fact]
    public void NotNullToNull_Should_Map()
    {
        var source = new A { NotNullToNull = 1 };
        var target = new B();

        _mapper.Map(source, target);

        Assert.Equal(1, target.NotNullToNull);
    }

    [Fact]
    public void NullToNotNull_Should_Ignore()
    {
        var source = new A { NullToNotNull = null };
        var target = new B();

        _mapper.Map(source, target);

        Assert.Equal(0, target.NullToNotNull);
    }

    [Fact]
    public void NullToNotNull_Should_Map()
    {
        var source = new A { NullToNotNull = 1 };
        var target = new B();

        _mapper.Map(source, target);

        Assert.Equal(1, target.NullToNotNull);
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