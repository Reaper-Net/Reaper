using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Reaper.SourceGenerator.ReaperEndpoints;

public static class ExtensionMethods
{
    public static bool IsEndpointTarget(this SyntaxNode node)
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

        return (node as ClassDeclarationSyntax).BaseList.Types.Any(m => m.Type.ToString().Contains("ReaperEndpoint"));
    }
}