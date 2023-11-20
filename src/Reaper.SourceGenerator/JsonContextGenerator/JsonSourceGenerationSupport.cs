using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json.SourceGeneration.Reaper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reaper.SourceGenerator.ReaperEndpoints;
using SourceGenerators;

namespace Reaper.SourceGenerator.JsonContextGenerator;

public static class JsonSourceGenerationSupport
{
    public static void RunJsonSourceGeneratorWithModifiedProvider(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<ImmutableArray<ReaperDefinition?>> endpointData)
    {
        var jsonSourceGenerator = new JsonSourceGenerator();

        var specs = endpointData
            .Select((symbol, token) =>
            {
                
                var comp = symbol.First().SemanticModel.Compilation;
                var ctxTree = GetSyntaxTreeForContext(comp, symbol);
                var classDeclaration = ctxTree.GetRoot().DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .First();
                comp = comp.AddSyntaxTrees(ctxTree);
                var wellKnown = new KnownTypeSymbols(comp);
                var model = comp.GetSemanticModel(ctxTree);
                JsonSourceGenerator.Parser parser = new(wellKnown);
                ContextGenerationSpec? contextGenerationSpec = parser.ParseContextGenerationSpec(classDeclaration, model, token);
                ImmutableEquatableArray<DiagnosticInfo> diagnostics = new ImmutableEquatableArray<DiagnosticInfo>(parser.Diagnostics);
                return (contextGenerationSpec, diagnostics);
            });
        
        context.RegisterSourceOutput(specs, ((productionContext, tuple) =>
        {
            JsonSourceGenerator.Emitter emitter = new(productionContext);
            emitter.Emit(tuple.contextGenerationSpec);
        }));
    }

    public static void RegisterAssemblyResolver()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name.Contains("SourceGeneration"))
            {
                var asm = Assembly.GetExecutingAssembly().GetManifestResourceStream("Reaper.SourceGenerator.Resources.System.Text.Json.SourceGeneration.Reaper.dll")!;
                var asmBytes = default(byte[]);
                using(var memoryStream = new MemoryStream())
                {
                    asm.CopyTo(memoryStream);
                    asmBytes = memoryStream.ToArray();
                }
#pragma warning disable RS1035
                return Assembly.Load(asmBytes);
#pragma warning restore RS1035
            }

            return null;
        };
    }

    public static SyntaxTree GetSyntaxTreeForContext(Compilation compilation, ImmutableArray<ReaperDefinition> endpoints)
    {   
        AttributeSyntax CreateJsonSerializableAttribute(string typeName)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Text.Json.Serialization.JsonSerializable"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(
                        SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(typeName)))
                    )
                ));
        }

        var attributes = new List<AttributeSyntax>();
        foreach (var endpoint in endpoints)
        {
            if (endpoint.HasRequest)
                attributes.Add(CreateJsonSerializableAttribute(endpoint.RequestTypeName));
            if (endpoint.HasResponse)
                attributes.Add(CreateJsonSerializableAttribute(endpoint.ResponseTypeName));
        }

        var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributes))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        var classDeclaration = SyntaxFactory.ClassDeclaration("ReaperJsonSerializerContext")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("System.Text.Json.Serialization.JsonSerializerContext")))
            .AddAttributeLists(attributeList);

        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Reaper.Generated"))
            .AddMembers(classDeclaration);

        var compUnit = SyntaxFactory.CompilationUnit()
            .AddMembers(namespaceDeclaration);
        var options = compilation.SyntaxTrees.First().Options as CSharpParseOptions;

        var syntaxTree = CSharpSyntaxTree.Create(compUnit, options);
        return syntaxTree;
    }
}