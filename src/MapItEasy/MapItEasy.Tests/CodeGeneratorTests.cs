namespace MapItEasy.Tests;

public class CodeGeneratorTests
{
    [Fact]
    public Task GenerateMappingCode_ReturnsExpectedCode()
    {
        // Act
        var code = CodeGenerator.GenerateMappingCode<A, B>();

        // Assert
        return Verify(code);
    }

    [Fact]
    public Task GenerateMappingCode_RespectsCustomVariableNames()
    {
        // Act
        var code = CodeGenerator.GenerateMappingCode<A, B>("src", "dest");

        // Assert
        return Verify(code);
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
