using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Reaper.SourceGenerator.Internal;
using Reaper.SourceGenerator.ReaperEndpoints;

namespace Reaper.SourceGenerator.JsonContextGenerator;

internal class JsonContextGenerator(ImmutableArray<ReaperDefinition> endpoints)
{
    private readonly CodeWriter codeWriter = new();

    public SourceText Generate()
    {
        codeWriter.AppendLine(GeneratorStatics.FileHeader);
        
        codeWriter.Namespace("Reaper.Generated");
        
        codeWriter.AppendLine("using System.Text.Json.Serialization;");
        codeWriter.AppendLine(string.Empty);

        foreach (var endpoint in endpoints.Where(m => m.HasRequest || m.HasResponse))
        {
            if (endpoint.HasRequest && endpoint.RequestSymbol!.ContainingNamespace is not { Name: "System" })
            {
                codeWriter.Append("[JsonSerializable(typeof(");
                codeWriter.Append(endpoint.RequestTypeName);
                codeWriter.AppendLine("))]");
            }

            if (endpoint.HasResponse && endpoint.ResponseSymbol!.ContainingNamespace is not { Name: "System" })
            {
                codeWriter.Append("[JsonSerializable(typeof(");
                codeWriter.Append(endpoint.ResponseTypeName);
                codeWriter.AppendLine("))]");
            }
        }
        
        codeWriter.StartClass("ReaperJsonSerializerContext", "public partial", "JsonSerializerContext", true);
        
        codeWriter.CloseBlock();
        codeWriter.CloseBlock();
        
        return SourceText.From(codeWriter.ToString(), Encoding.UTF8);
    }
}