using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Reaper.SourceGenerator.Internal;
using Reaper.SourceGenerator.ReaperEndpoints;

namespace Reaper.SourceGenerator.ServicesInterceptor;

internal class MapperInterceptorGenerator(ImmutableArray<ReaperDefinition> endpoints, IInvocationOperation mapperOperation)
{
    private readonly CodeWriter codeWriter = new();

    internal SourceText Generate()
    {
        var location = mapperOperation.GetInvocationLocation();
        
        codeWriter.AppendLine(GeneratorStatics.FileHeader);
        codeWriter.AppendLine(GeneratorStatics.CodeInterceptorAttribute);
        codeWriter.Namespace("Reaper.Generated");
        codeWriter.AppendLine("using System;");
        codeWriter.AppendLine("using System.Runtime.CompilerServices;");
        codeWriter.AppendLine("using Reaper.Mapper;");
        
        codeWriter.StartClass("MapperInterceptor", "file static");
        codeWriter.Append("[InterceptsLocation(\"");
        codeWriter.Append(location.file);
        codeWriter.Append("\", ");
        codeWriter.Append(location.line);
        codeWriter.Append(", ");
        codeWriter.Append(location.pos);
        codeWriter.AppendLine(")]");
        codeWriter.AppendLine("public static void MapReaperEndpoints_Impl(this WebApplication app)");
        codeWriter.OpenBlock();
        codeWriter.AppendLine("var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();");
        codeWriter.AppendLine("var logger = loggerFactory.CreateLogger(\"Reaper\");");
        codeWriter.AppendLine("var endpointLog = LoggerMessage.Define<Type>(LogLevel.Debug, new EventId(1, \"ReaperEndpoint\"), \"Reaper endpoint {EndpointClass} mapped w/ injection\");");
        codeWriter.AppendLine("Debug(\"💀 Reaper is mapping endpoints\");");
        codeWriter.AppendLine(string.Empty);

        var validEndpoints = endpoints.Where(m => m.IsFullyConfigured).ToList();
                
        foreach (var endpoint in validEndpoints)
        {
                    //Console.WriteLine(endpoint.TypeName + " req: " + endpoint.RequestTypeName + " res: " + endpoint.ResponseTypeName);
            if (endpoint.RequiresReaperHandler || (!endpoint.HasRequest && !endpoint.HasResponse))
            {
                codeWriter.Append($"ReaperMapper.MapEndpoint<");
                codeWriter.Append(endpoint.TypeName);
            } else if (endpoint.HasRequest && !endpoint.HasResponse)
            {
                codeWriter.Append($"ReaperMapper.MapEndpointWithRequest<");
                codeWriter.Append(endpoint.TypeName);
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.RequestTypeName);
            } else if (!endpoint.HasRequest && endpoint.HasResponse)
            {
                codeWriter.Append($"ReaperMapper.MapEndpointWithResponse<");
                codeWriter.Append(endpoint.TypeName);
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.ResponseTypeName);
            } else
            {
                codeWriter.Append($"ReaperMapper.MapEndpointWithRequestAndResponse<");
                codeWriter.Append(endpoint.TypeName);
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.RequestTypeName);
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.ResponseTypeName);
            }
            codeWriter.AppendLine(">(app, b => ");
            
            codeWriter.In();
            codeWriter.Append("b.WithRoute(\"");
            codeWriter.Append(endpoint.Route);
            codeWriter.AppendLine("\")");
            codeWriter.Append(" .WithVerb(\"");
            codeWriter.Append(endpoint.Verb);
            codeWriter.Append("\")");
            if (endpoint.HasRequest)
            {
                codeWriter.AppendLine(string.Empty);
                codeWriter.Append(" .WithRequest<");
                codeWriter.Append(endpoint.RequestTypeName);
                codeWriter.Append(">()");
            }
            if (endpoint.HasResponse)
            {
                codeWriter.AppendLine(string.Empty);
                codeWriter.Append(" .WithResponse<");
                codeWriter.Append(endpoint.ResponseTypeName);
                codeWriter.Append(">()");
            }
            if (endpoint.RequiresReaperHandler)
            {
                codeWriter.AppendLine(string.Empty);
                codeWriter.Append(" .WithReaperHandler()");
            }

            codeWriter.Out();
            codeWriter.AppendLine(");");

            codeWriter.Append("EndpointMapped<");
            codeWriter.Append(endpoint.TypeName);
            codeWriter.AppendLine(">();");
            codeWriter.AppendLine(string.Empty);
        }
        
        codeWriter.AppendLine("Debug(\"💀 {EndpointCount} Reaper endpoints mapped\", \"" + validEndpoints.Count + "\");");
        
        if (endpoints.Length != validEndpoints.Count)
        {
            foreach (var endpoint in endpoints.Where(m => !m.IsFullyConfigured))
            {
                codeWriter.Append("logger.LogWarning(\"💀❌  Reaper endpoint {EndpointClass} was misconfigured\", typeof(");
                codeWriter.Append(endpoint.TypeName);
                codeWriter.AppendLine("));");
            }
        }
        
        codeWriter.AppendLine("void Debug(string message, params string[] args) =>");
        codeWriter.In();
        codeWriter.AppendLine("logger.Log(LogLevel.Debug, message, args);");
        codeWriter.Out();
        codeWriter.AppendLine("void EndpointMapped<TEndpoint>() =>");
        codeWriter.In();
        codeWriter.AppendLine("endpointLog(logger, typeof(TEndpoint), null);");
        codeWriter.Out();
        
        codeWriter.CloseBlock();
        codeWriter.CloseBlock();
        codeWriter.CloseBlock();

        return SourceText.From(codeWriter.ToString(), Encoding.UTF8);
    }
}