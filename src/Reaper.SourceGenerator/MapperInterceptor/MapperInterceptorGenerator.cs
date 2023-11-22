using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Reaper.SourceGenerator.Internal;
using Reaper.SourceGenerator.ReaperEndpoints;

namespace Reaper.SourceGenerator.ServicesInterceptor;

internal class MapperInterceptorGenerator(ImmutableArray<ReaperDefinition> endpoints, IInvocationOperation mapperOperation)
{
    private readonly CodeWriter codeWriter = new();

    private void GenerateDirectReaperDefinition(ReaperDefinition endpoint)
    {
        codeWriter.AppendLine("app.MapReaperEndpoint(");
        codeWriter.In();
        codeWriter.AppendLine("new DirectReaperEndpointDefinition<");
        codeWriter.In();
        codeWriter.Append(endpoint.TypeName);
        codeWriter.AppendLine(",");
        codeWriter.Append(endpoint.HasRequest ? endpoint.RequestTypeName : "EmptyRequestResponse");
        codeWriter.AppendLine(",");
        codeWriter.Append(endpoint.HasRequest ? endpoint.RequestBodyTypeName : "EmptyRequestResponse");
        codeWriter.AppendLine(",");
        codeWriter.Append(endpoint.HasResponse ? endpoint.ResponseTypeName : "EmptyRequestResponse");
        codeWriter.Append(">(\"");
        codeWriter.Append(endpoint.Route);
        codeWriter.Append("\", \"");
        codeWriter.Append(endpoint.Verb);
        codeWriter.AppendLine("\")");
        codeWriter.Out();
        codeWriter.OpenBlock();
        codeWriter.Append("Handler = ");
        GenerateDirectReaperHandler(endpoint);
        codeWriter.Append("RequestDelegateGenerator = ");
        GenerateDirectReaperRequestDelegateGenerator(endpoint);
        codeWriter.Out();
        codeWriter.AppendLine("});");
        codeWriter.Out();
    }

    private void GenerateDirectReaperHandler(ReaperDefinition endpoint)
    {
        codeWriter.Append("async (");
        codeWriter.Append(endpoint.TypeName);
        codeWriter.Append(" svc");
        if (endpoint.HasRequest)
        {
            codeWriter.Append(", ");
            codeWriter.Append(endpoint.RequestTypeName);
            codeWriter.Append(" req");
        }
        codeWriter.Append(") => await svc.HandleAsync(");
        if (endpoint.HasRequest)
        {
            codeWriter.Append("req");
        }
        codeWriter.AppendLine("),");
    }

    private void GenerateDirectReaperRequestDelegateGenerator(ReaperDefinition endpoint)
    {
        codeWriter.AppendLine("(del, opts, _) =>");
        codeWriter.OpenBlock();
        codeWriter.AppendLine("var serviceProvider = (opts.ServiceProvider ?? opts.EndpointBuilder!.ApplicationServices)!;");
        codeWriter.AppendLine("var endpoint = serviceProvider.GetRequiredService<");
        codeWriter.Append(endpoint.TypeName);
        codeWriter.AppendLine(">();");
        if (endpoint.HasRequest || endpoint.HasResponse)
        {
            codeWriter.AppendLine("var logOrThrowExceptionHelper = new LogOrThrowExceptionHelper(serviceProvider, opts);");
            codeWriter.AppendLine("var jsonOptions = serviceProvider.GetService<IOptions<JsonOptions>>()?.Value ?? ReaperEndpointMapper.FallbackJsonOptions;");
            codeWriter.AppendLine("var jsonSerializerOptions = jsonOptions.SerializerOptions;");
            codeWriter.AppendLine("jsonSerializerOptions.MakeReadOnly();");

            if (endpoint.HasRequest)
            {
                codeWriter.Append("var jsonTypeInfoRequest = (JsonTypeInfo<");
                codeWriter.Append(endpoint.RequestBodyTypeName);
                codeWriter.Append(">)jsonSerializerOptions.GetTypeInfo(typeof(");
                codeWriter.Append(endpoint.RequestBodyTypeName);
                codeWriter.AppendLine("));");
            }

            if (endpoint.HasResponse)
            {
                codeWriter.Append("var jsonTypeInfoResponse = (JsonTypeInfo<");
                codeWriter.Append(endpoint.ResponseTypeName);
                codeWriter.Append("?>)jsonSerializerOptions.GetTypeInfo(typeof(");
                codeWriter.Append(endpoint.ResponseTypeName);
                codeWriter.AppendLine("));");
            }

            codeWriter.AppendLine(string.Empty);
            codeWriter.AppendLine("async Task Handler(HttpContext ctx)");
            codeWriter.OpenBlock();

            var acceptsBody = endpoint.Verb is "POST" or "PUT" or "PATCH";

            if (endpoint.HasRequest)
            {
                if (acceptsBody)
                {
                    codeWriter.Append("var requestParseResult = await JsonBodyResolver.TryResolveBodyAsync<");
                    codeWriter.Append(endpoint.RequestBodyTypeName);
                    codeWriter.Append(">(ctx, logOrThrowExceptionHelper, \"");
                    codeWriter.Append(endpoint.RequestMap!.RequestBodyType.Name);
                    codeWriter.Append("\", \"req\", jsonTypeInfoRequest");
                    if (ReferenceTypeIsNullable(endpoint.RequestMap.RequestBodyType))
                    {
                        codeWriter.Append(", false");
                    }
                    codeWriter.AppendLine(");");
                    codeWriter.AppendLine("if (!requestParseResult.Item1)");
                    codeWriter.OpenBlock();
                    codeWriter.AppendLine("return;");
                    codeWriter.CloseBlock();
                }

                if (!endpoint.RequestMap!.IsBoundRequest)
                {
                    // If there's no binding to be done, just proxy through the JSON from the request
                    if (acceptsBody)
                    {
                        codeWriter.AppendLine("var request = requestParseResult.Item2!;");
                    }
                }
                else
                {
                    // Otherwise, we need to construct the request object and fill it with the JSON, and anything else
                    var requestMap = endpoint.RequestMap!;
                    
                    codeWriter.Append("var request = Activator.CreateInstance<");
                    codeWriter.Append(endpoint.RequestTypeName);
                    codeWriter.AppendLine(">();");
                    if (acceptsBody && requestMap.BoundRequestBody)
                    {
                        codeWriter.AppendLine("// [FromBody]");
                        codeWriter.Append("request.");
                        codeWriter.Append(requestMap.RequestBodyProperty!.Name);
                        codeWriter.AppendLine(" = requestParseResult.Item2!;");
                    }

                    if (requestMap.BoundRoutes)
                    {
                        codeWriter.AppendLine("// [FromRoute]");
                        foreach (var boundRoute in requestMap.RouteProperties!)
                        {
                            GenerateFromXConverted("RouteValues", boundRoute.Key, boundRoute.Value);
                        }
                    }

                    if (requestMap.BoundQueries)
                    {
                        codeWriter.AppendLine("// [FromQuery]");
                        foreach (var boundQuery in requestMap.QueryProperties!)
                        {
                            GenerateFromXConverted("Query", boundQuery.Key, boundQuery.Value);
                        }
                    }
                }
            }

            if (endpoint.HasResponse)
            {
                codeWriter.Append("var response = await endpoint.HandleAsync(");
                if (endpoint.HasRequest)
                {
                    codeWriter.Append("request");
                }
                codeWriter.AppendLine(");");
                
                switch(endpoint.ResponseOptimisationType)
                {
                    case ResponseOptimisationType.None:
                        codeWriter.AppendLine("await ResponseHelpers.ExecuteReturnAsync(response, ctx, jsonTypeInfoResponse);");
                        break;
                    case ResponseOptimisationType.StringResponse:
                        codeWriter.AppendLine("await ctx.Response.WriteAsync(response);");
                        break;
                }
            }
            else
            {
                if (!endpoint.HasResponse)
                {
                    codeWriter.Append("await endpoint.HandleAsync(request);");
                }
            }
            codeWriter.CloseBlock();
        }
        else
        {
            codeWriter.AppendLine(string.Empty);
            codeWriter.AppendLine("Task Handler(HttpContext _) => endpoint.HandleAsync();");
        }
        
        codeWriter.AppendLine("return new RequestDelegateResult(Handler, ReadOnlyCollection<object>.Empty);");
        codeWriter.CloseBlock();
    }

    private bool ReferenceTypeIsNullable(ITypeSymbol type)
    {
        return type.NullableAnnotation == NullableAnnotation.Annotated;
    }

    private void GenerateFromXConverted(string from, string key, IPropertySymbol prop)
    {
        var useType = prop.Type;
        var nullableValueType = (useType.IsValueType && useType is INamedTypeSymbol && prop.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T);
        if (nullableValueType)
        {
            useType = ((INamedTypeSymbol) useType).TypeArguments[0];
        }
        
        codeWriter.Append("if (ctx.Request.");
        codeWriter.Append(from);
        codeWriter.Append(".TryGetValue(\"");
        codeWriter.Append(key);
        codeWriter.Append("\", out var ");
        codeWriter.Append(prop.Name);
        codeWriter.AppendLine("Src) && RequestHelpers.TryConvertValue<");
        codeWriter.Append(useType.Name);
        codeWriter.Append(">(");
        codeWriter.Append(prop.Name);
        codeWriter.Append("Src!, out var ");
        codeWriter.Append(prop.Name);
        codeWriter.AppendLine("Conv))");
        codeWriter.OpenBlock();
        codeWriter.Append("request.");
        codeWriter.Append(prop.Name);
        codeWriter.Append(" = (");
        codeWriter.Append(useType.Name);
        codeWriter.Append(")");
        codeWriter.Append(prop.Name);
        codeWriter.AppendLine("Conv!;");
        codeWriter.CloseBlock();
    }

    internal SourceText Generate()
    {
        var location = mapperOperation.GetInvocationLocation();
        var validEndpoints = endpoints.Where(m => m.IsFullyConfigured).ToList();
        
        codeWriter.AppendLine(GeneratorStatics.FileHeader);
        codeWriter.AppendLine(GeneratorStatics.CodeInterceptorAttribute);
        codeWriter.Namespace("Reaper.Generated");
        codeWriter.AppendLine("using Microsoft.AspNetCore.Http.Json;");
        codeWriter.AppendLine("using Microsoft.Extensions.Options;");
        codeWriter.AppendLine("using Reaper.EndpointMapper;");
        codeWriter.AppendLine("using System;");
        codeWriter.AppendLine("using System.Collections.Generic;");
        codeWriter.AppendLine("using System.Collections.ObjectModel;");
        codeWriter.AppendLine("using System.Runtime.CompilerServices;");
        codeWriter.AppendLine("using System.Text.Json.Serialization.Metadata;");
        if (validEndpoints.Any(m => !m.RequiresReaperHandler))
        {
            codeWriter.AppendLine("using Reaper.RequestDelegateSupport;");
        }
        
        codeWriter.AppendLine(string.Empty);
        
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

                
        foreach (var endpoint in validEndpoints)
        {
            // This is the simpler case, so handle it first
            if (endpoint.RequiresReaperHandler)
            {
                codeWriter.AppendLine("app.MapReaperEndpoint(");
                codeWriter.In();
                codeWriter.Append("new HandlerReaperEndpointDefinition<");
                codeWriter.Append(endpoint.TypeName);
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.HasRequest ? endpoint.RequestTypeName : "EmptyRequestResponse");
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.HasRequest ? endpoint.RequestBodyTypeName : "EmptyRequestResponse");
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.HasResponse ? endpoint.ResponseTypeName : "EmptyRequestResponse");
                codeWriter.Append(">(\"");
                codeWriter.Append(endpoint.Route);
                codeWriter.Append("\", \"");
                codeWriter.Append(endpoint.Verb);
                codeWriter.AppendLine("\"));");
            }
            else
            {
                // This is the minimal API mapping case and is extremely variable
                GenerateDirectReaperDefinition(endpoint);
            }
            
            codeWriter.Append("EndpointMapped<");
            codeWriter.Append(endpoint.TypeName);
            codeWriter.AppendLine(">();");
            codeWriter.AppendLine(string.Empty);

            continue;
            
            if (endpoint.RequiresReaperHandler || (!endpoint.HasRequest && !endpoint.HasResponse))
            {
                codeWriter.Append($"ReaperMapper.MapEndpoint<");
                codeWriter.Append(endpoint.TypeName);
            } else if (endpoint.HasRequest && !endpoint.HasResponse)
            {
                if (endpoint.RequestMap!.IsBoundRequest && endpoint.RequestMap!.BoundRequestBody)
                    codeWriter.Append("ReaperMapper.MapEndpointWithRequestBody<");
                else
                    codeWriter.Append($"ReaperMapper.MapEndpointWithRequest<");
                codeWriter.Append(endpoint.TypeName);
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.RequestTypeName);
                if (endpoint.RequestMap!.IsBoundRequest && endpoint.RequestMap!.BoundRequestBody)
                    codeWriter.Append(endpoint.RequestBodyTypeName);
            } else if (!endpoint.HasRequest && endpoint.HasResponse)
            {
                codeWriter.Append($"ReaperMapper.MapEndpointWithResponse<");
                codeWriter.Append(endpoint.TypeName);
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.ResponseTypeName);
            } else
            {
                if (endpoint.RequestMap!.IsBoundRequest && endpoint.RequestMap!.BoundRequestBody)
                    codeWriter.Append("ReaperMapper.MapEndpointWithRequestBodyAndResponse<");
                else
                    codeWriter.Append($"ReaperMapper.MapEndpointWithRequestAndResponse<");
                codeWriter.Append(endpoint.TypeName);
                codeWriter.Append(", ");
                codeWriter.Append(endpoint.RequestTypeName);
                if (endpoint.RequestMap!.IsBoundRequest && endpoint.RequestMap!.BoundRequestBody)
                {
                    codeWriter.Append(", ");
                    codeWriter.Append(endpoint.RequestBodyTypeName);
                }
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
                if (endpoint.RequestMap!.IsBoundRequest && endpoint.RequestMap!.BoundRequestBody)
                {
                    codeWriter.Append(" .WithRequestBody<");
                    codeWriter.Append(endpoint.RequestTypeName);
                    codeWriter.Append(", ");
                    codeWriter.Append(endpoint.RequestBodyTypeName);
                    codeWriter.Append(">((r, b) => r.");
                    codeWriter.Append(endpoint.RequestMap.RequestBodyProperty!.Name);
                    codeWriter.Append(" = b)");
                }
                else
                {
                    codeWriter.Append(" .WithRequest<");
                    codeWriter.Append(endpoint.RequestTypeName);
                    codeWriter.Append(">()");
                }
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