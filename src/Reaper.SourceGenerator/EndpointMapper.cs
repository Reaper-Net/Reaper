﻿using Microsoft.CodeAnalysis;
using Reaper.SourceGenerator.Internal;
using Reaper.SourceGenerator.JsonContextGenerator;
using Reaper.SourceGenerator.MapperInterceptor;
using Reaper.SourceGenerator.ServicesInterceptor;
using Reaper.SourceGenerator.ReaperEndpoints;

namespace Reaper.SourceGenerator
{
    [Generator]
    public class EndpointMapper : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            JsonSourceGenerationSupport.RegisterAssemblyResolver();
            
            var reaperEndpointDefinitions = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s.IsEndpointTarget(),
                    transform: static (ctx, ct) =>
                    {
                        var reaperDefinition = ctx.GetValidReaperDefinition(ct);
                        if (reaperDefinition.valid)
                        {
                            return reaperDefinition.definition!;
                        }

                        return null;
                    })
                .Where(m => m != null)
                .WithTrackingName(GeneratorStatics.StepEndpoints);

            var mapReaperEndpointCall = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => s.IsTargetForMapperInterceptor(),
                transform: static (ctx, ct) => ctx.TransformValidInvocation(ct))
                .Where(m => m != null)
                .WithTrackingName(GeneratorStatics.StepMapper);
            
            var useReaperCall = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => s.IsTargetForServicesGenerator(),
                    transform: static (ctx, ct) => ctx.TransformValidInvocation(ct))
                .Where(m => m != null)
                .WithTrackingName(GeneratorStatics.StepServices);

            var collectedEndpoints = reaperEndpointDefinitions.Collect();
            
            context.RunJsonSourceGeneratorWithModifiedProvider(collectedEndpoints);
            
            var allData = collectedEndpoints
                .Combine(mapReaperEndpointCall.Collect().Select((x, _) => x.First()))
                .Combine(useReaperCall.Collect().Select((x, _) => x.First()));

            context.RegisterSourceOutput(allData, (ctx, data) =>
            {
                var ((endpoints, mapReaper), useReaper) = data;

                var jsonContextGenerator = new JsonContextGenerator.JsonContextGenerator(endpoints);
                var jsonContextCode = jsonContextGenerator.Generate();
                ctx.AddSource("ReaperJsonSerializerContext.Base.g.cs", jsonContextCode);
                
                var serviceInterceptorGenerator = new ServicesInterceptorGenerator(endpoints, useReaper);
                var serviceInterceptorCode = serviceInterceptorGenerator.Generate();
                ctx.AddSource("ServicesInterceptor.g.cs", serviceInterceptorCode);
                
                var mapperInterceptorGenerator = new MapperInterceptorGenerator(endpoints, mapReaper);
                var mapperInterceptorCode = mapperInterceptorGenerator.Generate();
                ctx.AddSource("MapperInterceptor.g.cs", mapperInterceptorCode);
            });
        }
    }
}