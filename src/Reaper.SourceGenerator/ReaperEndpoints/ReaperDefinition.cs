using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reaper.SourceGenerator.ReaperEndpoints;

public record ReaperDefinition(ClassDeclarationSyntax ClassDeclarationSyntax, SemanticModel SemanticModel, INamedTypeSymbol Symbol)
{
    public ITypeSymbol? RequestSymbol { get; init; }
    public bool HasRequest => RequestSymbol != null;
    
    public ITypeSymbol? ResponseSymbol { get; init; }
    public bool HasResponse => ResponseSymbol != null;
    
    public AttributeData? RouteAttribute { get; init; }
    public bool HasRouteAttribute => RouteAttribute != null;
    
    public bool IsScoped { get; init; }
    public bool IsForceUseHandler { get; init; }

    public bool IsFullyConfigured => HasRouteAttribute;
    public bool RequiresReaperHandler => IsForceUseHandler;

    private string? typeName;
    public string TypeName => typeName ??= Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    
    private string? requestTypeName;
    public string RequestTypeName => requestTypeName ??= RequestSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
    
    private string? responseTypeName;
    public string ResponseTypeName => responseTypeName ??= ResponseSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
    
    private string? route;
    public string Route => route ??= RouteAttribute?.ConstructorArguments[1].Value?.ToString() ?? string.Empty;
    
    private string? verb;
    public string Verb => verb ??= RouteAttribute?.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
}