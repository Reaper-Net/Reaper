using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reaper.SourceGenerator.ServicesInterceptor;

public static class ExtensionMethods
{
    public static bool IsTargetForServicesGenerator(this SyntaxNode node)
    {
        return node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name: SimpleNameSyntax
                {
                    Identifier: SyntaxToken
                    {
                        ValueText: "UseReaper"
                    }
                }
            }
        };
    }
}