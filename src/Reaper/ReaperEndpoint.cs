using Microsoft.AspNetCore.Http;
using Reaper.Context;

namespace Reaper;

public interface IReaperEndpointHasRequest {}
public interface IReaperEndpointHasResponse {}

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
    
    protected HttpContext Context => reaperExecutionContextProvider.Context.HttpContext;
}

public abstract class ReaperEndpoint : ReaperEndpointBase
{
    public abstract Task HandleAsync();
}

// ReSharper disable once InconsistentNaming
public abstract class ReaperEndpointRX<TRequest> : ReaperEndpointBase, IReaperEndpointHasRequest
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