using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MapItEasy.Generators;

[Generator]
public class MappingSourceGenerator : IIncrementalGenerator
{
    private const string GeneratedMappingAttributeFullName = "MapItEasy.GeneratedMappingAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GeneratedMappingAttributeFullName,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => GetMethodInfo(ctx, ct))
            .Where(static m => m is not null);

        var grouped = methodDeclarations.Collect();

        context.RegisterSourceOutput(grouped, static (spc, methods) => Execute(spc, methods!));
    }

    private static bool IsMappingOptionsType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.OriginalDefinition.ToDisplayString() == "MapItEasy.MappingOptions<TSource>")
        {
            return true;
        }

        return false;
    }

    private static MethodMappingInfo? GetMethodInfo(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var methodSymbol = context.TargetSymbol as IMethodSymbol;
        if (methodSymbol == null)
            return null;

        if (!methodSymbol.IsPartialDefinition)
            return null;

        var containingType = methodSymbol.ContainingType;
        if (containingType == null)
            return null;

        // Must not be generic — we need concrete types to enumerate properties
        if (methodSymbol.TypeParameters.Length != 0)
            return null;

        // Determine source type from first parameter
        if (methodSymbol.Parameters.Length < 1)
            return null;

        var sourceType = methodSymbol.Parameters[0].Type as INamedTypeSymbol;
        if (sourceType == null)
            return null;

        // Check for MappingOptions parameter (can be the last parameter)
        bool hasOptionsParameter = false;
        string? optionsParameterName = null;
        int nonOptionsParamCount = 0;

        for (int i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var param = methodSymbol.Parameters[i];
            if (IsMappingOptionsType(param.Type))
            {
                hasOptionsParameter = true;
                optionsParameterName = param.Name;
            }
            else
            {
                nonOptionsParamCount++;
            }
        }

        bool isVoidOverload = methodSymbol.ReturnsVoid;
        INamedTypeSymbol? targetType = null;

        if (isVoidOverload)
        {
            // void Map(Source source, Target target, ...) or void Map(Source source, Target target, MappingOptions<Source> options)
            if (nonOptionsParamCount < 2)
                return null;
            targetType = methodSymbol.Parameters[1].Type as INamedTypeSymbol;
        }
        else
        {
            // Target Map(Source source, ...) or Target Map(Source source, MappingOptions<Source> options)
            targetType = methodSymbol.ReturnType as INamedTypeSymbol;
        }

        if (targetType == null)
            return null;

        // Check if this is an extension method by looking at the syntax for a 'this' modifier on the first parameter
        bool isExtensionMethod = methodSymbol.IsExtensionMethod;

        // Collect property mappings
        var sourceProps = GetAllPublicProperties(sourceType);
        var targetProps = GetAllPublicProperties(targetType);

        var mappings = new List<PropertyMapping>();

        foreach (var sourceProp in sourceProps)
        {
            var targetProp = targetProps.FirstOrDefault(p => p.Name == sourceProp.Name);
            if (targetProp == null)
                continue;

            if (targetProp.SetMethod == null || targetProp.SetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (sourceProp.GetMethod == null || sourceProp.GetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            var sourceUnderlyingType = GetUnderlyingType(sourceProp.Type);
            var targetUnderlyingType = GetUnderlyingType(targetProp.Type);

            // Only map if underlying types are the same
            if (!SymbolEqualityComparer.Default.Equals(sourceUnderlyingType, targetUnderlyingType))
                continue;

            bool sourceIsNullable = IsNullableValueType(sourceProp.Type);
            bool targetIsNullable = IsNullableValueType(targetProp.Type);

            mappings.Add(new PropertyMapping(
                sourceProp.Name,
                sourceProp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                targetProp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                sourceIsNullable,
                targetIsNullable));
        }

        var containingTypeNamespace = GetNamespace(containingType);

        // Collect the using directives from the containing syntax tree for the partial class
        var paramNames = methodSymbol.Parameters.Select(p => p.Name).ToList();

        return new MethodMappingInfo(
            containingTypeNamespace,
            containingType.Name,
            containingType.IsStatic,
            methodSymbol.Name,
            sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            isVoidOverload,
            methodSymbol.DeclaredAccessibility,
            mappings,
            paramNames,
            methodSymbol,
            isExtensionMethod,
            hasOptionsParameter,
            optionsParameterName);
    }

    private static List<IPropertySymbol> GetAllPublicProperties(INamedTypeSymbol type)
    {
        var properties = new List<IPropertySymbol>();
        var current = type;
        while (current != null)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop && prop.DeclaredAccessibility == Accessibility.Public && !prop.IsStatic && !prop.IsIndexer)
                {
                    if (!properties.Any(p => p.Name == prop.Name))
                        properties.Add(prop);
                }
            }
            current = current.BaseType;
        }
        return properties;
    }

    private static ITypeSymbol GetUnderlyingType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            namedType.TypeArguments.Length == 1)
        {
            return namedType.TypeArguments[0];
        }
        return type;
    }

    private static bool IsNullableValueType(ITypeSymbol type)
    {
        return type is INamedTypeSymbol namedType &&
               namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    private static string GetNamespace(INamedTypeSymbol type)
    {
        var ns = type.ContainingNamespace;
        if (ns == null || ns.IsGlobalNamespace)
            return "";
        return ns.ToDisplayString();
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<MethodMappingInfo?> methods)
    {
        if (methods.IsDefaultOrEmpty)
            return;

        var grouped = methods
            .Where(m => m != null)
            .GroupBy(m => new { m!.ContainingTypeNamespace, m.ContainingTypeName });

        foreach (var group in grouped)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            var ns = group.Key.ContainingTypeNamespace;
            if (!string.IsNullOrEmpty(ns))
            {
                sb.AppendLine($"namespace {ns};");
                sb.AppendLine();
            }

            var first = group.First()!;
            var staticModifier = first.ContainingTypeIsStatic ? "static " : "";
            var accessibility = SyntaxFacts.GetText(first.Accessibility);

            sb.AppendLine($"{accessibility} {staticModifier}partial class {group.Key.ContainingTypeName}");
            sb.AppendLine("{");

            foreach (var method in group)
            {
                if (method == null) continue;
                GenerateMethod(sb, method);
            }

            sb.AppendLine("}");

            var hintName = string.IsNullOrEmpty(ns)
                ? $"{group.Key.ContainingTypeName}.g.cs"
                : $"{ns}.{group.Key.ContainingTypeName}.g.cs";

            context.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    private static void GenerateMethod(StringBuilder sb, MethodMappingInfo method)
    {
        var accessibilityText = SyntaxFacts.GetText(method.Accessibility);

        // Reconstruct parameter list from the original method
        var paramList = BuildParameterList(method);

        if (method.IsVoidOverload)
        {
            var sourceName = method.ParameterNames.Count > 0 ? method.ParameterNames[0] : "source";
            var targetName = method.ParameterNames.Count > 1 ? method.ParameterNames[1] : "target";

            sb.AppendLine($"    {accessibilityText} static partial void {method.MethodName}({paramList})");
            sb.AppendLine("    {");

            GenerateNullGuards(sb, sourceName, targetName);

            if (method.HasOptionsParameter)
            {
                GenerateOptionsHandling(sb, method, sourceName, targetName, isVoidOverload: true);
            }
            else
            {
                GenerateAssignments(sb, method.Mappings, sourceName, targetName);
            }

            sb.AppendLine("    }");
        }
        else
        {
            var sourceName = method.ParameterNames.Count > 0 ? method.ParameterNames[0] : "source";

            sb.AppendLine($"    {accessibilityText} static partial {method.TargetTypeFullName} {method.MethodName}({paramList})");
            sb.AppendLine("    {");

            sb.AppendLine($"        if ({sourceName} == null)");
            sb.AppendLine($"            throw new global::System.ArgumentNullException(nameof({sourceName}));");
            sb.AppendLine();
            sb.AppendLine($"        var target = new {method.TargetTypeFullName}();");
            sb.AppendLine();

            if (method.HasOptionsParameter)
            {
                GenerateOptionsHandling(sb, method, sourceName, "target", isVoidOverload: false);
            }
            else
            {
                GenerateAssignments(sb, method.Mappings, sourceName, "target");
            }

            sb.AppendLine();
            sb.AppendLine("        return target;");
            sb.AppendLine("    }");
        }

        sb.AppendLine();
    }

    private static void GenerateOptionsHandling(StringBuilder sb, MethodMappingInfo method, string sourceName, string targetName, bool isVoidOverload)
    {
        var optionsName = method.OptionsParameterName ?? "options";
        var returnStatement = isVoidOverload ? "return;" : $"return {targetName};";

        sb.AppendLine($"        if ({optionsName} != null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            {optionsName}.Validate();");
        sb.AppendLine();
        sb.AppendLine($"            if ({optionsName}.IncludeProperties != null)");
        sb.AppendLine("            {");
        sb.AppendLine($"                var __includeNames = {optionsName}.IncludeProperties;");
        sb.AppendLine();

        foreach (var mapping in method.Mappings)
        {
            sb.AppendLine($"                if (__includeNames.Contains(\"{mapping.PropertyName}\"))");
            sb.AppendLine("                {");
            GenerateSingleAssignment(sb, mapping, sourceName, targetName, "                    ");
            sb.AppendLine("                }");
        }

        sb.AppendLine();
        sb.AppendLine($"                {returnStatement}");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine($"            if ({optionsName}.ExcludeProperties != null)");
        sb.AppendLine("            {");
        sb.AppendLine($"                var __excludeNames = {optionsName}.ExcludeProperties;");
        sb.AppendLine();

        foreach (var mapping in method.Mappings)
        {
            sb.AppendLine($"                if (!__excludeNames.Contains(\"{mapping.PropertyName}\"))");
            sb.AppendLine("                {");
            GenerateSingleAssignment(sb, mapping, sourceName, targetName, "                    ");
            sb.AppendLine("                }");
        }

        sb.AppendLine();
        sb.AppendLine($"                {returnStatement}");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();

        // Default: map all properties (when options is null or no include/exclude specified)
        GenerateAssignments(sb, method.Mappings, sourceName, targetName);
    }

    private static void GenerateSingleAssignment(StringBuilder sb, PropertyMapping mapping, string sourceName, string targetName, string indent)
    {
        if (mapping.SourceIsNullable && !mapping.TargetIsNullable)
        {
            sb.AppendLine($"{indent}if ({sourceName}.{mapping.PropertyName}.HasValue)");
            sb.AppendLine($"{indent}    {targetName}.{mapping.PropertyName} = {sourceName}.{mapping.PropertyName}.Value;");
        }
        else
        {
            sb.AppendLine($"{indent}{targetName}.{mapping.PropertyName} = {sourceName}.{mapping.PropertyName};");
        }
    }

    private static string BuildParameterList(MethodMappingInfo method)
    {
        // Reconstruct from the original method symbol
        var parts = new List<string>();
        var sym = method.MethodSymbol;
        for (int i = 0; i < sym.Parameters.Length; i++)
        {
            var param = sym.Parameters[i];
            var typeName = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var nullable = param.NullableAnnotation == NullableAnnotation.Annotated && !IsNullableValueType(param.Type) ? "?" : "";
            var thisModifier = (i == 0 && method.IsExtensionMethod) ? "this " : "";

            if (param.HasExplicitDefaultValue)
            {
                var defaultVal = param.ExplicitDefaultValue == null ? "null" : param.ExplicitDefaultValue.ToString();
                parts.Add($"{thisModifier}{typeName}{nullable} {param.Name} = {defaultVal}");
            }
            else
            {
                parts.Add($"{thisModifier}{typeName}{nullable} {param.Name}");
            }
        }
        return string.Join(", ", parts);
    }

    private static void GenerateNullGuards(StringBuilder sb, string sourceName, string targetName)
    {
        sb.AppendLine($"        if ({sourceName} == null)");
        sb.AppendLine($"            throw new global::System.ArgumentNullException(nameof({sourceName}));");
        sb.AppendLine($"        if ({targetName} == null)");
        sb.AppendLine($"            throw new global::System.ArgumentNullException(nameof({targetName}));");
        sb.AppendLine();
    }

    private static void GenerateAssignments(StringBuilder sb, List<PropertyMapping> mappings, string sourceName, string targetName)
    {
        foreach (var mapping in mappings)
        {
            if (mapping.SourceIsNullable && !mapping.TargetIsNullable)
            {
                // Nullable source -> non-nullable target: only assign if source has value
                sb.AppendLine($"        if ({sourceName}.{mapping.PropertyName}.HasValue)");
                sb.AppendLine($"            {targetName}.{mapping.PropertyName} = {sourceName}.{mapping.PropertyName}.Value;");
            }
            else if (!mapping.SourceIsNullable && mapping.TargetIsNullable)
            {
                // Non-nullable source -> nullable target: implicit conversion
                sb.AppendLine($"        {targetName}.{mapping.PropertyName} = {sourceName}.{mapping.PropertyName};");
            }
            else
            {
                // Same nullability
                sb.AppendLine($"        {targetName}.{mapping.PropertyName} = {sourceName}.{mapping.PropertyName};");
            }
        }
    }
}

internal class PropertyMapping
{
    public string PropertyName { get; }
    public string SourceTypeFullName { get; }
    public string TargetTypeFullName { get; }
    public bool SourceIsNullable { get; }
    public bool TargetIsNullable { get; }

    public PropertyMapping(string propertyName, string sourceTypeFullName, string targetTypeFullName, bool sourceIsNullable, bool targetIsNullable)
    {
        PropertyName = propertyName;
        SourceTypeFullName = sourceTypeFullName;
        TargetTypeFullName = targetTypeFullName;
        SourceIsNullable = sourceIsNullable;
        TargetIsNullable = targetIsNullable;
    }
}

internal class MethodMappingInfo
{
    public string ContainingTypeNamespace { get; }
    public string ContainingTypeName { get; }
    public bool ContainingTypeIsStatic { get; }
    public string MethodName { get; }
    public string SourceTypeFullName { get; }
    public string TargetTypeFullName { get; }
    public bool IsVoidOverload { get; }
    public Accessibility Accessibility { get; }
    public List<PropertyMapping> Mappings { get; }
    public List<string> ParameterNames { get; }
    public IMethodSymbol MethodSymbol { get; }
    public bool IsExtensionMethod { get; }
    public bool HasOptionsParameter { get; }
    public string? OptionsParameterName { get; }

    public MethodMappingInfo(
        string containingTypeNamespace,
        string containingTypeName,
        bool containingTypeIsStatic,
        string methodName,
        string sourceTypeFullName,
        string targetTypeFullName,
        bool isVoidOverload,
        Accessibility accessibility,
        List<PropertyMapping> mappings,
        List<string> parameterNames,
        IMethodSymbol methodSymbol,
        bool isExtensionMethod,
        bool hasOptionsParameter,
        string? optionsParameterName)
    {
        ContainingTypeNamespace = containingTypeNamespace;
        ContainingTypeName = containingTypeName;
        ContainingTypeIsStatic = containingTypeIsStatic;
        MethodName = methodName;
        SourceTypeFullName = sourceTypeFullName;
        TargetTypeFullName = targetTypeFullName;
        IsVoidOverload = isVoidOverload;
        Accessibility = accessibility;
        Mappings = mappings;
        ParameterNames = parameterNames;
        MethodSymbol = methodSymbol;
        IsExtensionMethod = isExtensionMethod;
        HasOptionsParameter = hasOptionsParameter;
        OptionsParameterName = optionsParameterName;
    }
}
