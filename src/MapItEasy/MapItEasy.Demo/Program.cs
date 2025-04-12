using MapItEasy;

IMapper mapper = new ReflectionMapper();
//IMapper mapper = new ExpressionMapper();

var test1 = mapper.Map<A, B>(new A { Id = 1, Name = "abc1", Description = "xyz1" });

var test2 = new B();
mapper.Map(new A { Id = 2, Name = "abc2", Description = "xyz2" }, test2);

Console.ReadLine();


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
