using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reaper.SourceGenerator.MapperInterceptor;

public static class ExtensionMethods
{
    public static bool IsTargetForMapperInterceptor(this SyntaxNode node)
    {
        return node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name: SimpleNameSyntax
                {
                    Identifier: SyntaxToken
                    {
                        ValueText: "MapReaperEndpoints"
                    }
                }
            }
        };
    }
}