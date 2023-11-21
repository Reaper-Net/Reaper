using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Reaper.SourceGenerator.RoslynHelpers;

namespace Reaper.SourceGenerator.ReaperEndpoints;

internal static class ExtensionMethods
{
    internal static bool IsEndpointTarget(this SyntaxNode node)
    {
        var sc = node is ClassDeclarationSyntax
        {
            BaseList:
            {
                Types:
                {
                    Count: > 0,
                },
            },
        };
        if (!sc)
        {
            return false;
        }

        return (node as ClassDeclarationSyntax)!.BaseList!.Types.Any(m => m.Type.ToString().Contains("ReaperEndpoint"));
    }
    
    internal static IInvocationOperation? TransformValidInvocation(this GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var reaperOperation = ctx.GetValidReaperInvokationOperation(ct);
        if (reaperOperation.valid)
        {
            return reaperOperation.operation;
        }

        return null;
    }
    
    internal static (string file, int line, int pos) GetInvocationLocation(this IInvocationOperation operation)
    {
        var memberAccessorExpression = ((MemberAccessExpressionSyntax)((InvocationExpressionSyntax)operation.Syntax).Expression);
        var invocationNameSpan = memberAccessorExpression.Name.Span;
        var lineSpan = operation.Syntax.SyntaxTree.GetLineSpan(invocationNameSpan);
        var origFilePath = operation.Syntax.SyntaxTree.FilePath;
        var filePath =operation.SemanticModel?.Compilation.Options.SourceReferenceResolver?.NormalizePath(origFilePath, baseFilePath: null) ?? origFilePath;
        // LineSpan.LinePosition is 0-indexed, but we want to display 1-indexed line and character numbers in the interceptor attribute.
        return (filePath, lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1);
    }
    
    internal static (bool valid, ReaperDefinition? definition) GetValidReaperDefinition(this GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var wellKnownTypes = WellKnownTypes.GetOrCreate(ctx.SemanticModel.Compilation);
        var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, ct) as INamedTypeSymbol;
        var baseSymbol = symbol?.BaseType;
        var baseBaseSymbol = baseSymbol?.BaseType;
        if (baseBaseSymbol?.Equals(wellKnownTypes.ReaperEndpointBase, SymbolEqualityComparer.Default) == true)
        {
            bool isRequest = !baseSymbol!.EqualsWithoutGeneric(wellKnownTypes.ReaperEndpointXR);
            ITypeSymbol? request = default, response = default;
            
            foreach (var typeArg in baseSymbol!.TypeArguments)
            {
                if (isRequest)
                {
                    request = typeArg;
                    isRequest = false;
                }
                else
                {
                    response = typeArg;
                }
            }

            var attributes = symbol!.GetAttributes();
            AttributeData? routeAttribute = default;
            bool isScoped = false, isForceHandler = false;
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass?.Equals(wellKnownTypes.ReaperRouteAttribute, SymbolEqualityComparer.Default) == true)
                {
                    routeAttribute = attribute;
                } else if (attribute.AttributeClass?.Equals(wellKnownTypes.ReaperScopedAttribute, SymbolEqualityComparer.Default) == true)
                {
                    isScoped = true;
                } else if (attribute.AttributeClass?.Equals(wellKnownTypes.ReaperForceHandlerAttribute, SymbolEqualityComparer.Default) == true)
                {
                    isForceHandler = true;
                }
            }

            RequestTypeMap? requestTypeMap = default!;

            if (request != null)
            {
                requestTypeMap = new RequestTypeMap(request, wellKnownTypes);
            }

            var optimisationType = ResponseOptimisationType.None;
            if (response != null)
            {
                if (response.Equals(wellKnownTypes.StringType, SymbolEqualityComparer.Default))
                {
                    optimisationType = ResponseOptimisationType.StringResponse;
                }
            }

            return (true, new ReaperDefinition((ctx.Node as ClassDeclarationSyntax)!, ctx.SemanticModel, symbol, baseSymbol)
            {
                ResponseOptimisationType = optimisationType,
                RequestMap = requestTypeMap,
                ResponseSymbol = response,
                RouteAttribute = routeAttribute,
                IsScoped = isScoped,
                IsForceUseHandler = isForceHandler
            });
        }
        return (false, default);
    }
}