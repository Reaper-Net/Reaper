using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Reaper.SourceGenerator.RoslynHelpers;

public static class ExtensionMethods
{
    public static (bool valid, IInvocationOperation? operation) GetValidReaperInvokationOperation(this GeneratorSyntaxContext ctx, CancellationToken ct)
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
            return (true, (IInvocationOperation?)operation);
        }

        return (false, null);
    }

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Symbols should be compared for equality",
        Justification = "Symbol equality checks for generic type equality.")]
    public static bool EqualsWithoutGeneric(this INamedTypeSymbol symbol, INamedTypeSymbol compare)
    {
        if (!Equals(symbol.ContainingNamespace, compare.ContainingNamespace))
            return false;

        if (symbol.Name != compare.Name)
            return false;

        if (symbol.IsGenericType != compare.IsGenericType)
            return false;

        return true;
    }
}