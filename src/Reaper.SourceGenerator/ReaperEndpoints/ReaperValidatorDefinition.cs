using Microsoft.CodeAnalysis;

namespace Reaper.SourceGenerator.ReaperEndpoints;

public record ReaperValidatorDefinition
{
    public INamedTypeSymbol? Validator { get; init; }
    public ITypeSymbol? RequestSymbol { get; init; }
}