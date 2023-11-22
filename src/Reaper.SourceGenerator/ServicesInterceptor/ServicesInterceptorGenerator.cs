using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Reaper.SourceGenerator.Internal;
using Reaper.SourceGenerator.ReaperEndpoints;

namespace Reaper.SourceGenerator.ServicesInterceptor;

internal class ServicesInterceptorGenerator(ImmutableArray<ReaperDefinition> endpoints, IInvocationOperation servicesOperation)
{
    private readonly CodeWriter codeWriter = new();

    internal SourceText Generate()
    {
        var location = servicesOperation.GetInvocationLocation();
        
        codeWriter.AppendLine(GeneratorStatics.FileHeader);
        codeWriter.AppendLine(GeneratorStatics.CodeInterceptorAttribute);
        codeWriter.Namespace("Reaper.Generated");
        codeWriter.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        codeWriter.AppendLine("using System.Runtime.CompilerServices;");
        codeWriter.AppendLine("using Reaper.Context;");
        codeWriter.StartClass("ServiceAdditionInterceptor", "file static");
        codeWriter.Append("[InterceptsLocation(\"");
        codeWriter.Append(location.file);
        codeWriter.Append("\", ");
        codeWriter.Append(location.line);
        codeWriter.Append(", ");
        codeWriter.Append(location.pos);
        codeWriter.AppendLine(")]");
        codeWriter.AppendLine("public static void UseReaper_Impl(this WebApplicationBuilder app)");
        codeWriter.AppendLine("{");
        codeWriter.In();
        codeWriter.AppendLine("app.Services.TryAddSingleton<IReaperExecutionContextProvider, ReaperExecutionContextProvider>();");
        codeWriter.AppendLine(string.Empty);

        if (endpoints.Any(m => m.HasRequest || m.HasResponse))
        {
            codeWriter.AppendLine("app.Services.ConfigureHttpJsonOptions(options =>");
            codeWriter.In();
            codeWriter.AppendLine("{");
            codeWriter.In();
            codeWriter.AppendLine("options.SerializerOptions.TypeInfoResolverChain.Insert(0, ReaperJsonSerializerContext.Default);");
            codeWriter.Out();
            codeWriter.AppendLine("});");
            codeWriter.Out();
        }
        
        codeWriter.AppendLine("// Endpoints");
                
        var validEndpoints = endpoints.Where(m => m.IsFullyConfigured).ToList();

        foreach (var endpoint in validEndpoints)
        {
            codeWriter.Append("app.Services.TryAddSingleton<");
            codeWriter.Append(endpoint.TypeName);
            codeWriter.AppendLine(">();");
        }

        codeWriter.CloseBlock();
        codeWriter.CloseBlock();
        codeWriter.CloseBlock();

        return SourceText.From(codeWriter.ToString(), Encoding.UTF8);
    }
}