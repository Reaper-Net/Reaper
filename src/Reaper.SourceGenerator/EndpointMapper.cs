using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reaper.SourceGenerator
{
    [Generator]
    public class EndpointMapper : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var reaperEndpointClasses = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => Matcher(s),
                    transform: static (ctx, _) => Transformer(ctx))
                .Where(m => m.BaseTypeName.AsSpan(0, 21).SequenceEqual("Reaper.ReaperEndpoint".AsSpan())) // We don't do a symbol comparison here as it could be a sub-type
                .WithTrackingName("Syntax");
            
            var mapReaperEndpointCall = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => s is InvocationExpressionSyntax inv &&
                                           inv.Expression is MemberAccessExpressionSyntax mae &&
                                           mae.Name.ToString() == "MapReaperEndpoints",
                transform: static (ctx, _) => ctx.Node)
                .WithTrackingName("MapReaperEndpoint");
            
            var useReaperCall = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => s is InvocationExpressionSyntax inv &&
                                                inv.Expression is MemberAccessExpressionSyntax mae &&
                                                mae.Name.ToString() == "UseReaper",
                    transform: static (ctx, _) => ctx.Node)
                .WithTrackingName("UseReaper");
            
            var epsWithCompilation = context.CompilationProvider.Combine(reaperEndpointClasses.Collect());

            List<EndpointDataHolder> endpointData = new();
            
            // This doesn't actually output anything, it's just to get the endpoint names
            context.RegisterSourceOutput(epsWithCompilation, (spc, epComp) =>
            {
                var compilation = epComp.Left;
                
                var reaperRouteAttribute = compilation.GetTypeByMetadataName("Reaper.Attributes.ReaperRouteAttribute");
                var reaperScopedAttribute = compilation.GetTypeByMetadataName("Reaper.Attributes.ReaperRouteAttribute");
                var reaperRequestOnly = compilation.GetTypeByMetadataName("Reaper.ReaperEndpointRX`1");
                var reaperResponseOnly = compilation.GetTypeByMetadataName("Reaper.ReaperEndpointXR`1");
                var reaperRequestResponse = compilation.GetTypeByMetadataName("Reaper.ReaperEndpoint`2");
                
                var eshs = epComp.Right;
                if (eshs.IsDefaultOrEmpty) return;

                foreach (var esh in eshs)
                {
                    var cds = esh.ClassDeclaration;

                    string? verb = default;
                    string? route = default;
                    string? requestType = default;
                    string? responseType = default;
                    
                    var classSymbol = compilation.GetSemanticModel(cds.SyntaxTree).GetDeclaredSymbol(cds);
                    var baseClass = classSymbol?.BaseType;
                    var isRequest = true;
                    if (baseClass.TypeArguments.Length == 1)
                    {
                        if (reaperResponseOnly.Name == baseClass.Name)
                        {
                            isRequest = false;
                        }
                    }
                    foreach (var types in baseClass.TypeArguments)
                    {
                        var typeName = types.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        
                        if (isRequest)
                        {
                            requestType = typeName;
                            isRequest = false;
                        }
                        else
                        {
                            responseType = typeName;
                        }
                    }
                    
                    /*var baseType = cds.BaseList!.Types[0];
                    var baseTypeSymbol = compilation.GetSemanticModel(baseType.SyntaxTree);
                    baseTypeSymbol.GetDeclaredSymbol(baseType).TypeP*/
                    
                    var attrs = classSymbol.GetAttributes();
                    foreach (var attr in attrs)
                    {
                        //Console.WriteLine(attr.AttributeClass.Name);
                        if (reaperRouteAttribute.Equals(attr.AttributeClass, SymbolEqualityComparer.Default))
                        {
                            verb = attr.ConstructorArguments[0].Value.ToString();
                            route = attr.ConstructorArguments[1].Value.ToString();
                        }
                    }

                    if (verb != default && route != default)
                    {
                        endpointData.Add(new EndpointDataHolder
                        {
                            TypeName = esh.FullTypeName,
                            Verb = verb,
                            Route = route,
                            RequestTypeName = requestType,
                            ResponseTypeName = responseType
                        });
                    }
                    else
                    {
                        endpointData.Add(new EndpointDataHolder
                        {
                            TypeName = esh.FullTypeName,
                            IsMisConfigured = true
                        });
                    }
                }
            });

            context.RegisterSourceOutput(useReaperCall, ((productionContext, node) =>
            {
                var fileLocation = node.SyntaxTree.FilePath;
                var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
                var fileLine = lineSpan.StartLinePosition.Line + 1; // 1 indexed
                var callStart = node.ToString().IndexOf("UseReaper", StringComparison.Ordinal) + 1;

                var sb = new StringBuilder();
                var pl = (string str, int x = 0) =>
                {
                    sb.Append(new string(' ', x * 4));
                    sb.AppendLine(str);
                };
                var p = (string str, int x = 0) =>
                {
                    sb.Append(new string(' ', x * 4));
                    sb.Append(str);
                };
                sb.AppendLine(StaticGeneration.FileHeader);
                sb.AppendLine(StaticGeneration.CodeInterceptorAttribute);

                pl("namespace Reaper.Generated {");
                pl("using Microsoft.Extensions.DependencyInjection.Extensions;", 1);
                pl("using System.Runtime.CompilerServices;", 1);
                pl("using Reaper.Context;", 1);
                sb.AppendLine();
                pl(StaticGeneration.GeneratedCodeAttribute, 1);
                pl("file static class ServiceAdditionInterceptor", 1);
                pl("{", 1);
                p("[InterceptsLocation(\"", 2);
                sb.Append(fileLocation);
                sb.Append("\", ");
                sb.Append(fileLine);
                sb.Append(", ");
                sb.Append(callStart);
                sb.AppendLine(")]");
                pl("public static void UseReaper_Impl(this WebApplicationBuilder app)", 2);
                pl("{", 2);
                
                pl("app.Services.TryAddSingleton<IReaperExecutionContextProvider, ReaperExecutionContextProvider>();", 3);
                pl("", 3);

                pl("// Endpoints", 3);
                
                var validEndpoints = endpointData.Where(m => !m.IsMisConfigured).ToList();

                foreach (var endpoint in validEndpoints)
                {
                    p("app.Services.AddReaperEndpoint<", 3);
                    sb.Append(endpoint.TypeName);
                    sb.AppendLine(">();");
                }

                pl("}", 2);
                pl("}", 1);
                pl("}");

                productionContext.AddSource("ServicesInterceptor.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));
            }));

            context.RegisterSourceOutput(mapReaperEndpointCall, ((productionContext, node) =>
            {
                var fileLocation = node.SyntaxTree.FilePath;
                var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
                var fileLine = lineSpan.StartLinePosition.Line + 1; // 1 indexed
                var callStart = node.ToString().IndexOf("MapReaperEndpoints", StringComparison.Ordinal) + 1;
                
                var sb = new StringBuilder();
                var pl = (string str, int x = 0) => {
                    sb.Append(new string(' ', x * 4));
                    sb.AppendLine(str);
                };
                var p = (string str, int x = 0) =>
                {
                    sb.Append(new string(' ', x * 4));
                    sb.Append(str);
                };
                sb.AppendLine(StaticGeneration.FileHeader);
                sb.AppendLine(StaticGeneration.CodeInterceptorAttribute);

                pl("namespace Reaper.Generated {"); 
                pl("using System;", 1);
                pl("using System.Runtime.CompilerServices;", 1);
                pl("using Reaper.Mapper;", 1);
                sb.AppendLine();
                pl(StaticGeneration.GeneratedCodeAttribute, 1);
                pl("file static class EndpointMapperInterceptor", 1);
                pl("{", 1);
                p("[InterceptsLocation(\"", 2);
                sb.Append(fileLocation);
                sb.Append("\", ");
                sb.Append(fileLine);
                sb.Append(", ");
                sb.Append(callStart);
                sb.AppendLine(")]");
                pl("public static void MapReaperEndpoints_Impl(this WebApplication app)", 2);
                pl("{", 2);
                pl("var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();", 3);
                pl("var logger = loggerFactory.CreateLogger(\"Reaper\");", 3);
                pl("var endpointLog = LoggerMessage.Define<Type>(LogLevel.Debug, new EventId(1, \"ReaperEndpoint\"), \"Reaper endpoint {EndpointClass} mapped w/ injection\");", 3);
                pl("Debug(\"💀 Reaper is mapping endpoints\");", 3);
                sb.AppendLine();

                var validEndpoints = endpointData.Where(m => !m.IsMisConfigured).ToList();
                
                foreach (var endpoint in validEndpoints)
                {
                    //Console.WriteLine(endpoint.TypeName + " req: " + endpoint.RequestTypeName + " res: " + endpoint.ResponseTypeName);
                    if (endpoint.RequiresReaperHandler || (endpoint.RequestTypeName == null && endpoint.ResponseTypeName == null))
                    {
                        p($"ReaperMapper.MapEndpoint<", 3);
                        sb.Append(endpoint.TypeName);
                        sb.AppendLine(">(app, b => ");
                        p("b.WithRoute(\"", 4);
                        sb.Append(endpoint.Route);
                        sb.AppendLine("\")");
                    } else if (endpoint.RequestTypeName != null && endpoint.ResponseTypeName == null)
                    {
                        p($"ReaperMapper.MapEndpointWithRequest<", 3);
                        sb.Append(endpoint.TypeName);
                        sb.Append(", ");
                        sb.Append(endpoint.RequestTypeName);
                        sb.AppendLine(">(app, b => ");
                        p("b.WithRoute(\"", 4);
                        sb.Append(endpoint.Route);
                        sb.AppendLine("\")");
                    } else if (endpoint.RequestTypeName == null && endpoint.ResponseTypeName != null)
                    {
                        p($"ReaperMapper.MapEndpointWithResponse<", 3);
                        sb.Append(endpoint.TypeName);
                        sb.Append(", ");
                        sb.Append(endpoint.ResponseTypeName);
                        sb.AppendLine(">(app, b => ");
                        p("b.WithRoute(\"", 4);
                        sb.Append(endpoint.Route);
                        sb.AppendLine("\")");
                    } else
                    {
                        p($"ReaperMapper.MapEndpointWithRequestAndResponse<", 3);
                        sb.Append(endpoint.TypeName);
                        sb.Append(", ");
                        sb.Append(endpoint.RequestTypeName);
                        sb.Append(", ");
                        sb.Append(endpoint.ResponseTypeName);
                        sb.AppendLine(">(app, b => ");
                        p("b.WithRoute(\"", 4);
                        sb.Append(endpoint.Route);
                        sb.AppendLine("\")");
                    }

                    p(" .WithVerb(\"", 4);
                    sb.Append(endpoint.Verb);
                    sb.Append("\")");
                    if (endpoint.RequestTypeName is not null)
                    {
                        sb.AppendLine();
                        p(" .WithRequest<", 4);
                        sb.Append(endpoint.RequestTypeName);
                        sb.Append(">()");
                    }
                    if (endpoint.ResponseTypeName is not null)
                    {
                        sb.AppendLine();
                        p(" .WithResponse<", 4);
                        sb.Append(endpoint.ResponseTypeName);
                        sb.Append(">()");
                    }
                    if (endpoint.RequiresReaperHandler)
                    {
                        sb.AppendLine();
                        p(" .WithReaperHandler()", 4);
                    }
                    else
                    {
                        
                    }
                    pl(");");

                    p("EndpointMapped<", 3);
                    sb.Append(endpoint.TypeName);
                    sb.AppendLine(">();");

                    sb.AppendLine();
                }

                p("logger.LogInformation(\"💀 {EndpointCount} Reaper endpoints mapped\", ", 3);
                sb.Append(validEndpoints.Count);
                sb.AppendLine(");");

                if (endpointData.Count != validEndpoints.Count)
                {
                    foreach (var endpoint in endpointData.Where(m => m.IsMisConfigured))
                    {
                        p("logger.LogWarning(\"💀❌  Reaper endpoint {EndpointClass} was misconfigured\", typeof(", 3);
                        sb.Append(endpoint.TypeName);
                        sb.AppendLine("));");
                    }
                }

                sb.AppendLine();
                
                pl("void Debug(string message, params string[] args) => ", 3);
                pl("logger.Log(LogLevel.Debug, message, args);", 4);
                pl("void EndpointMapped<TEndpoint>() => ", 3);
                pl("endpointLog(logger, typeof(TEndpoint), null);", 4);
                
                pl("}", 2);
                pl("}", 1);
                sb.AppendLine("}");
                
                productionContext.AddSource("MapperInterceptor.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));
            }));
        }
        
        static bool Matcher(SyntaxNode node)
        {
            if (!node.IsKind(SyntaxKind.ClassDeclaration)) return false;
            var c = (ClassDeclarationSyntax)node;
            if (c.BaseList?.Types.Count is null or 0) return false;
            return c.BaseList!.Types.Any(m => m.Type.ToString().Contains("ReaperEndpoint"));
        }
        
        static EndpointSyntaxHolder Transformer(GeneratorSyntaxContext context)
        {
            var cds = (ClassDeclarationSyntax)context.Node;
            var fullTypeName = context.SemanticModel.GetDeclaredSymbol(cds)!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var baseTypeName = context.SemanticModel.GetTypeInfo(cds.BaseList!.Types[0].Type).Type!.ToString(); // We suppress nullable here as it's already been pre-vetted
            return new EndpointSyntaxHolder(cds, baseTypeName, fullTypeName);
        }
    }
    
    public record EndpointSyntaxHolder(ClassDeclarationSyntax ClassDeclaration, string BaseTypeName, string FullTypeName);

    public record EndpointDataHolder
    {
        public string TypeName { get; init; } = default!;
        
        public bool IsMisConfigured { get; set; }

        public string Route { get; init; } = default!;

        public string Verb { get; init; } = default!;
        
        public bool RequiresReaperHandler { get; init; }
        
        public string? RequestTypeName { get; init; }
        public string? ResponseTypeName { get; init; }
    }
}