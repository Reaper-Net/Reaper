using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

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

    public static (bool, IOperation?) IsValidUseReaperOperation(this GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var operation = ctx.SemanticModel.GetOperation(ctx.Node, ct);
        if (operation == null)
        {
            return (false, null);
        }

        if (operation is IInvocationOperation
            {
                TargetMethod:
                {
                    ContainingNamespace:
                    {
                        Name: "Reaper"
                    },
                    ContainingAssembly:
                    {
                        Name: "Reaper"
                    }
                }
            })
        {
            return (true, operation);
        }

        return (false, operation);
    }
}