using Microsoft.CodeAnalysis;
// ReSharper disable InconsistentNaming

namespace Reaper.SourceGenerator.RoslynHelpers;

internal class WellKnownTypes(Compilation compilation)
{
    private readonly Lazy<INamedTypeSymbol> reaperEndpoint = new(() => compilation.GetTypeByMetadataName("Reaper.ReaperEndpoint")!);
    internal INamedTypeSymbol ReaperEndpoint => reaperEndpoint.Value;
    
    private readonly Lazy<INamedTypeSymbol> reaperEndpointRR = new(() => compilation.GetTypeByMetadataName("Reaper.ReaperEndpoint`2")!);
    internal INamedTypeSymbol ReaperEndpointRR => reaperEndpointRR.Value;
    
    private readonly Lazy<INamedTypeSymbol> reaperEndpointRX = new(() => compilation.GetTypeByMetadataName("Reaper.ReaperEndpointRX`1")!);
    internal INamedTypeSymbol ReaperEndpointRX => reaperEndpointRX.Value;
    
    private readonly Lazy<INamedTypeSymbol> reaperEndpointXR = new(() => compilation.GetTypeByMetadataName("Reaper.ReaperEndpointXR`1")!);
    internal INamedTypeSymbol ReaperEndpointXR => reaperEndpointXR.Value;
    
    private readonly Lazy<INamedTypeSymbol> reaperEndpointBase = new(() => compilation.GetTypeByMetadataName("Reaper.ReaperEndpointBase")!);
    internal INamedTypeSymbol ReaperEndpointBase => reaperEndpointBase.Value;
    
    private readonly Lazy<INamedTypeSymbol> reaperRouteAttribute = new(() => compilation.GetTypeByMetadataName("Reaper.Attributes.ReaperRouteAttribute")!);
    internal INamedTypeSymbol ReaperRouteAttribute => reaperRouteAttribute.Value;
    
    private readonly Lazy<INamedTypeSymbol> reaperScopedAttribute = new(() => compilation.GetTypeByMetadataName("Reaper.Attributes.ReaperScopedAttribute")!);
    internal INamedTypeSymbol ReaperScopedAttribute => reaperScopedAttribute.Value;
    
    private readonly Lazy<INamedTypeSymbol> reaperForceHandlerAttribute = new(() => compilation.GetTypeByMetadataName("Reaper.Attributes.ReaperForceHandlerAttribute")!);
    internal INamedTypeSymbol ReaperForceHandlerAttribute => reaperForceHandlerAttribute.Value;

    private readonly Lazy<INamedTypeSymbol> aspnetFromBodyAttribute = new(() => compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromBodyAttribute")!);
    internal INamedTypeSymbol AspNetFromBodyAttribute => aspnetFromBodyAttribute.Value;
    
    private readonly Lazy<INamedTypeSymbol> aspnetFromRouteAttribute = new(() => compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromRouteAttribute")!);
    internal INamedTypeSymbol AspNetFromRouteAttribute => aspnetFromRouteAttribute.Value;
    
    private readonly Lazy<INamedTypeSymbol> aspnetFromQueryAttribute = new(() => compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromQueryAttribute")!);
    internal INamedTypeSymbol AspNetFromQueryAttribute => aspnetFromQueryAttribute.Value;
    
    private readonly Lazy<INamedTypeSymbol> stringType = new(() => compilation.GetTypeByMetadataName("System.String")!);
    internal INamedTypeSymbol StringType => stringType.Value;

    
    private static WellKnownTypes? instance;
    internal static WellKnownTypes GetOrCreate(Compilation compilation)
    {
        if (instance == default)
        {
            instance = new WellKnownTypes(compilation);
        }

        return instance;
    }
}