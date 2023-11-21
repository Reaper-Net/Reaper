using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reaper.SourceGenerator.RoslynHelpers;

namespace Reaper.SourceGenerator.ReaperEndpoints;

internal record ReaperDefinition(ClassDeclarationSyntax ClassDeclarationSyntax, SemanticModel SemanticModel, INamedTypeSymbol Symbol, INamedTypeSymbol BaseSymbol)
{
    public required ResponseOptimisationType ResponseOptimisationType { get; init; }
    
    public RequestTypeMap? RequestMap { get; init; }
    public bool HasRequest => RequestMap != null;
    
    public ITypeSymbol? ResponseSymbol { get; init; }
    public bool HasResponse => ResponseSymbol != null;
    
    public AttributeData? RouteAttribute { get; init; }
    public bool HasRouteAttribute => RouteAttribute != null;
    
    private string? baseTypeName;
    public string BaseTypeName => baseTypeName ??= BaseSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    
    public bool IsScoped { get; init; }
    public bool IsForceUseHandler { get; init; }

    public bool IsFullyConfigured => HasRouteAttribute;
    public bool RequiresReaperHandler => IsForceUseHandler;

    private string? typeName;
    public string TypeName => typeName ??= Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    
    private string? requestTypeName;
    public string RequestTypeName => requestTypeName ??= RequestMap?.RequestType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
    
    private string? requestBodyTypeName;
    public string RequestBodyTypeName => requestBodyTypeName ??= RequestMap?.RequestBodyType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
    
    private string? responseTypeName;
    public string ResponseTypeName => responseTypeName ??= ResponseSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
    
    private string? route;
    public string Route => route ??= RouteAttribute?.ConstructorArguments[1].Value?.ToString() ?? string.Empty;
    
    private string? verb;
    public string Verb => verb ??= RouteAttribute?.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
}

public enum ResponseOptimisationType
{
    None,
    StringResponse
}

internal record RequestTypeMap
{
    public bool IsBoundRequest { get; set; }
    
    public ITypeSymbol RequestType { get; init; }
    public ITypeSymbol RequestBodyType { get; init; }
    public IPropertySymbol? RequestBodyProperty { get; init; }
    public bool BoundRequestBody => RequestBodyProperty != null;
    
    public ImmutableDictionary<string, IPropertySymbol>? RouteProperties { get; init; }
    public bool BoundRoutes => RouteProperties != null;
    public ImmutableDictionary<string, IPropertySymbol>? QueryProperties { get; init; }
    public bool BoundQueries => QueryProperties != null;
    
    public ImmutableArray<IPropertySymbol> Properties { get; init; }
    
    internal RequestTypeMap(ITypeSymbol type, WellKnownTypes wkt)
    {
        RequestType = type;
        RequestBodyType = type;
        Properties = type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public)
            .ToImmutableArray();
        
        var requestBodyProperties = Properties.Where(m => m.GetAttributes().Any(attr => attr.AttributeClass!.Equals(wkt.AspNetFromBodyAttribute, SymbolEqualityComparer.Default)));
        // We take the first one, need to implement diagnostics to alert the user that there's more than 1
        var requestBodyProperty = requestBodyProperties.FirstOrDefault();
        if (requestBodyProperty != null)
        {
            RequestBodyProperty = requestBodyProperty;
            RequestBodyType = requestBodyProperty.Type;
            IsBoundRequest = true;
        }
        
        // Now we want to check if there's any [FromRoute] or [FromQuery]
        // TODO [FromForm]???
        var routeOrQueryProperties = Properties.Select(m =>
            new
            {
                Property = m,
                RouteAttribute = m.GetAttributes().FirstOrDefault(attr => attr.AttributeClass!.Equals(wkt.AspNetFromRouteAttribute, SymbolEqualityComparer.Default)),
                QueryAttribute = m.GetAttributes().FirstOrDefault(attr => attr.AttributeClass!.Equals(wkt.AspNetFromQueryAttribute, SymbolEqualityComparer.Default))
            })
            .ToImmutableArray();
        
        var routeAttributeProperties = routeOrQueryProperties.Where(m => m.RouteAttribute != null).ToImmutableArray();
        var queryAttributeProperties = routeOrQueryProperties.Where(m => m.QueryAttribute != null).ToImmutableArray();
        if (routeAttributeProperties.Length > 0)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, IPropertySymbol>();
            foreach (var routeAttributeProperty in routeAttributeProperties)
            {
                // Determine if we're using the property name or the name specified in the attribute
                var name = routeAttributeProperty.RouteAttribute!.NamedArguments.FirstOrDefault(m => m.Key == "Name").Value.Value?.ToString() ??
                           routeAttributeProperty.Property.Name;
                builder.Add(name, routeAttributeProperty.Property);
            }
            RouteProperties = builder.ToImmutable();
            IsBoundRequest = true;
        }

        if (queryAttributeProperties.Length > 0)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, IPropertySymbol>();
            foreach (var queryAttributeProperty in queryAttributeProperties)
            {
                // Determine if we're using the property name or the name specified in the attribute
                var name = queryAttributeProperty.QueryAttribute!.NamedArguments.FirstOrDefault(m => m.Key == "Name").Value.Value?.ToString() ??
                           queryAttributeProperty.Property.Name;
                builder.Add(name, queryAttributeProperty.Property);
            }
            QueryProperties = builder.ToImmutable();
            IsBoundRequest = true;
        }
    }
}