using MapItEasy;

IMapper mapper = new ReflectionMapper();
//IMapper mapper = new ExpressionMapper();

var email = new Email
{
    Id = 1,
    From = "abc1",
    To = "xyz1"
};

ReturnNewObject();
MapExistingObject();

var code = CodeGenerator.GenerateMappingCode<Email, ArchivedEmail>("source", "target");

Console.WriteLine(code);

CodeGenerator.GenerateMappingFile<Email, ArchivedEmail>();

Console.ReadLine();

void ReturnNewObject()
{
    var archivedEmail1 = mapper.Map<Email, ArchivedEmail>(email);
}

void MapExistingObject()
{
    var archivedEmail = new ArchivedEmail();
    mapper.Map(email, archivedEmail);
}

public class Email
{
    public int Id { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public string Body { get; set; }
}

public class ArchivedEmail
{
    public int Id { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public string Body { get; set; }
}
