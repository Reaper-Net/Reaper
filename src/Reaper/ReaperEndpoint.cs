using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Reaper.Context;

namespace Reaper;

public interface IReaperEndpoint
{
    void SetContextProvider(IReaperExecutionContextProvider provider);
}

public abstract class ReaperEndpointBase : IReaperEndpoint
{
    private IReaperExecutionContextProvider? reaperExecutionContextProvider;

    public void SetContextProvider(IReaperExecutionContextProvider provider)
    {
        reaperExecutionContextProvider = provider;
    }

    protected HttpContext Context => reaperExecutionContextProvider!.Context.HttpContext;

    protected HttpRequest Request => Context.Request;
    protected HttpResponse Response => Context.Response;

    protected T Resolve<T>() where T : notnull => reaperExecutionContextProvider!.Context.HttpContext.RequestServices.GetRequiredService<T>();
}

public abstract class ReaperEndpoint : ReaperEndpointBase
{
    public abstract Task HandleAsync();
}

// ReSharper disable once InconsistentNaming
public abstract class ReaperEndpointRX<TRequest> : ReaperEndpointBase
{
    public abstract Task HandleAsync(TRequest request);
}

// ReSharper disable once InconsistentNaming
public abstract class ReaperEndpointXR<TResponse> : ReaperEndpointBase
{
    public abstract Task<TResponse> HandleAsync();
}

public abstract class ReaperEndpoint<TRequest, TResponse> : ReaperEndpointBase
{
    public abstract Task<TResponse> HandleAsync(TRequest request);
}