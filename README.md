Simple and fast object mapper (built using Expression Trees API) to map data between 2 objects which have identical (or nearly identical) shapes.

## User Cases:
- Cloning.
- Data archiving (moving data from the active table to the archived table).

## Examples
```c#
IMapper _mapper = new ExpressionMapper();
```
### Map all possible properties
```c#
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
```

### Map all possible selected properties
```c#
[Fact]
public void MapProperties()
{
	var source = new A { Id = 1, Name = "abc1", Description = "xyz1" };
	var target = new B();

	_mapper.MapProperties(source, target, x => new { x.Name });

	Assert.Equal(0, target.Id);
	Assert.Equal("abc1", target.Name);
	Assert.Null(target.Description);
}
```

### Map all possible properties except selected properties
```c#
[Fact]
public void MapExclude()
{
	var source = new A { Id = 1, Name = "abc1", Description = "xyz1" };
	var target = new B();

	_mapper.MapExclude(source, target, x => new { x.Name });

	Assert.Equal(1, target.Id);
	Assert.Null(target.Name);
	Assert.Equal("xyz1", target.Description);
}
```

## License
**MapItEasy** is licensed under the [MIT](/LICENSE) license.