using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Reaper.SourceGenerator.RoslynHelpers;

public static class ExtensionMethods
{
    public static (bool, IOperation?) GetValidReaperOperation(this GeneratorSyntaxContext ctx, CancellationToken ct)
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