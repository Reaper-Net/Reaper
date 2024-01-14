using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Reaper.Context;
using Reaper.Response;
using Reaper.Validation;

namespace Reaper;

public interface IReaperEndpoint
{
    void SetContextProvider(IReaperExecutionContextProvider provider);

    IReaperExecutionContext ReaperExecutionContext { get; }
    HttpContext Context { get; }
    HttpRequest Request { get; }
    HttpResponse Response { get; }
    IReaperValidationContext ValidationContext { get; }
}

public abstract class ReaperEndpointBase : IReaperEndpoint
{
    private IReaperExecutionContextProvider? reaperExecutionContextProvider;

    public void SetContextProvider(IReaperExecutionContextProvider provider)
    {
        reaperExecutionContextProvider = provider;
    }

    public IReaperExecutionContext ReaperExecutionContext => reaperExecutionContextProvider!.Context;
    public HttpContext Context => reaperExecutionContextProvider!.Context.HttpContext;

    public HttpRequest Request => Context.Request;
    public HttpResponse Response => Context.Response;

    public IReaperValidationContext ValidationContext => ReaperExecutionContext.ValidationContext;

    protected T Resolve<T>() where T : notnull => reaperExecutionContextProvider!.Context.HttpContext.RequestServices.GetRequiredService<T>();

    protected async Task Ok() => await ReaperEndpointExtensions.Ok(this);
    protected async Task Ok<T>(T? result) => await ReaperEndpointExtensions.Ok(this, result);
    
    protected async Task BadRequest() => await ReaperEndpointExtensions.BadRequest(this);
    protected async Task BadRequest<T>(T? result) => await ReaperEndpointExtensions.BadRequest(this, result);
    
    protected async Task NotFound() => await ReaperEndpointExtensions.NotFound(this);
    protected async Task NotFound<T>(T? result) => await ReaperEndpointExtensions.NotFound(this, result);
    
    protected async Task InternalServerError() => await ReaperEndpointExtensions.InternalServerError(this);
    protected async Task InternalServerError<T>(T? result) => await ReaperEndpointExtensions.InternalServerError(this, result);
    
    protected async Task StatusCode(int statusCode) => await ReaperEndpointExtensions.StatusCode(this, statusCode);
    protected async Task StatusCode<T>(int statusCode, T? result) => await ReaperEndpointExtensions.StatusCode(this, statusCode, result);
}

public abstract class ReaperEndpoint : ReaperEndpointBase
{
    public abstract Task ExecuteAsync();
}

// ReSharper disable once InconsistentNaming
public abstract class ReaperEndpointRX<TRequest> : ReaperEndpointBase
{
    public abstract Task ExecuteAsync(TRequest request);
}

// ReSharper disable once InconsistentNaming
public abstract class ReaperEndpointXR<TResponse> : ReaperEndpointBase
{
    public TResponse? Result { get; protected set; }
    
    public abstract Task ExecuteAsync();
}

public abstract class ReaperEndpoint<TRequest, TResponse> : ReaperEndpointBase
{
    public TResponse? Result { get; protected set; }
    
    public abstract Task ExecuteAsync(TRequest request);
}