using System.Text;

namespace MapItEasy;

public class CodeGenerator
{
    private static readonly Dictionary<(Type From, Type To), List<string>> _cache = [];

    public static string GenerateMappingMethods<TSource, TTarget>(string sourceVariableName, string targetVariableName)
    {
        var fromType = typeof(TSource);
        var toType = typeof(TTarget);

        var fromProps = typeof(TSource).GetProperties();
        var toProps = typeof(TTarget).GetProperties();

        var method1 = new StringBuilder();
        method1.AppendLine($"public void MapTo{toType.Name}({toType.Name} {targetVariableName})");
        method1.AppendLine("{");

        var method2 = new StringBuilder();
        method2.AppendLine($"public {toType.Name} Create{toType.Name}()");
        method2.AppendLine("{");
        method2.AppendLine($"\treturn new {toType.Name} " + "{");

        var method3 = new StringBuilder();
        method3.AppendLine($"public void From{fromType.Name}({fromType.Name} {sourceVariableName})");
        method3.AppendLine("{");

        var method4 = new StringBuilder();
        method4.AppendLine($"public static {toType.Name} Create{toType.Name}From{fromType.Name}({fromType.Name} {sourceVariableName})");
        method4.AppendLine("{");
        method4.AppendLine($"\treturn new {toType.Name} " + "{");

        var method5 = new StringBuilder();
        method5.AppendLine($"public static void Map{fromType.Name}To{toType.Name}({fromType.Name} {sourceVariableName}, {toType.Name} {targetVariableName})");
        method5.AppendLine("{");

        foreach (var from in fromProps)
        {
            var to = toProps.FirstOrDefault(x => x.Name == from.Name);

            if (to == null)
            {
                continue;
            }

            if (!from.IsAssignableTo(to))
            {
                continue;
            }

            if (from.GetMethod == null || to.SetMethod == null)
            {
                continue;
            }

            var name = from.Name;

            method1.Append($"\t{targetVariableName}.{name} = {name};{Environment.NewLine}");
            method2.Append($"\t\t{name} = {name},{Environment.NewLine}");
            method3.Append($"\t{name} = {sourceVariableName}.{name};{Environment.NewLine}");
            method4.Append($"\t\t{name} = {sourceVariableName}.{name},{Environment.NewLine}");
            method5.Append($"\t{targetVariableName}.{name} = {sourceVariableName}.{name};{Environment.NewLine}");
        }

        method1.AppendLine("}");

        method2.AppendLine("\t};");
        method2.AppendLine("}");

        method3.AppendLine("}");

        method4.AppendLine("\t};");
        method4.AppendLine("}");

        method5.AppendLine("}");


        var class1 = new StringBuilder();
        class1.AppendLine($"public partial class {fromType.Name}");
        class1.AppendLine("{");
        class1.AppendLine(string.Join("\n", method1.ToString().Split('\n').Select(x => $"\t{x}")));
        class1.AppendLine(string.Join("\n", method2.ToString().Split('\n').Select(x => $"\t{x}")));
        class1.AppendLine("}");

        var class2 = new StringBuilder();
        class2.AppendLine($"public partial class {toType.Name}");
        class2.AppendLine("{");
        class2.AppendLine(string.Join("\n", method3.ToString().Split('\n').Select(x => $"\t{x}")));
        class2.AppendLine("}");

        var class3 = new StringBuilder();
        class3.AppendLine($"public partial class MappingExtensions");
        class3.AppendLine("{");
        class3.AppendLine(string.Join("\n", method4.ToString().Split('\n').Select(x => $"\t{x}")));
        class3.AppendLine(string.Join("\n", method5.ToString().Split('\n').Select(x => $"\t{x}")));
        class3.AppendLine("}");

        return class1.ToString()
            + Environment.NewLine
            + class2.ToString()
            + Environment.NewLine
            + class3.ToString();
    }
}
