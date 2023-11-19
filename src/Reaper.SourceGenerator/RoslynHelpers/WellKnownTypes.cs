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
    
    private readonly Lazy<INamedTypeSymbol> reaperRouteAttribute = new(() => compilation.GetTypeByMetadataName("Reaper.Attributes.ReaperRouteAttribute")!);
    internal INamedTypeSymbol ReaperRouteAttribute => reaperRouteAttribute.Value;
    
    private readonly Lazy<INamedTypeSymbol> reaperScopedAttribute = new(() => compilation.GetTypeByMetadataName("Reaper.Attributes.ReaperRouteAttribute")!);
    internal INamedTypeSymbol ReaperScopedAttribute => reaperScopedAttribute.Value;
    
    private static WellKnownTypes? instance;
    static WellKnownTypes GetOrCreate(Compilation compilation)
    {
        if (instance == default)
        {
            instance = new WellKnownTypes(compilation);
        }

        return instance;
    }
}