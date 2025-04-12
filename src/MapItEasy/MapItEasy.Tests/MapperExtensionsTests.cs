namespace MapItEasy.Tests;

public class MapperExtensionsTests
{
    [Fact]
    public void ReturnNewObject()
    {
        var source = new A { Id = 1, Name = "abc1", Description = "xyz1" };

        var target = source.Map<A, B>();

        Assert.Equal(1, target.Id);
        Assert.Equal("abc1", target.Name);
        Assert.Equal("xyz1", target.Description);
    }

    [Fact]
    public void MapExistingObject()
    {
        var source = new A { Id = 1, Name = "abc1", Description = "xyz1" };
        var target = new B();

        source.Map(target);

        Assert.Equal(1, target.Id);
        Assert.Equal("abc1", target.Name);
        Assert.Equal("xyz1", target.Description);
    }

    public class A
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class B
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}