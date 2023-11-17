using System.Collections.Immutable;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reaper.Context;

namespace Reaper;

public static class WebApplicationBuilderExtensions
{
    public static void UseReaper(this WebApplicationBuilder builder)
    {
        throw new InvalidProgramException("Reaper Source Generator Interceptors not operative.");
    }

    public static void AddReaperEndpoint<TEndpoint>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TEndpoint: ReaperEndpointBase, new()
    {
        services.TryAdd(new ServiceDescriptor(typeof(TEndpoint), s =>
        {
            var ep = new TEndpoint();
            ep.SetContextProvider(s.GetRequiredService<IReaperExecutionContextProvider>());
            return ep;
        }, lifetime));
    }
}