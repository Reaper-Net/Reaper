using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reaper.RequestDelegateSupport;

namespace Reaper.Response;

public static class ReaperEndpointExtensions
{
    public static async Task StatusCode(this IReaperEndpoint endpoint, int statusCode)
    {
        endpoint.Response.StatusCode = statusCode;
    }
    
    public static async Task StatusCode<T>(this IReaperEndpoint endpoint, int statusCode, T? result = default)
    {
        await endpoint.StatusCode(statusCode);
        await ExecuteResponseAsync(endpoint.Context, result);
    }
    
    public static Task Ok(this IReaperEndpoint endpoint)
    {
        return endpoint.StatusCode(StatusCodes.Status200OK);
    }
    
    public static async Task Ok<T>(this IReaperEndpoint endpoint, T? result = default)
    {
        await endpoint.Ok();
        await ExecuteResponseAsync(endpoint.Context, result);
    }

    public static Task BadRequest(this IReaperEndpoint endpoint)
    {
        return endpoint.StatusCode(StatusCodes.Status400BadRequest);
    }
    
    public static async Task BadRequest<T>(this IReaperEndpoint endpoint, T? result = default)
    {
        await endpoint.BadRequest();
        await ExecuteResponseAsync(endpoint.Context, result);
    }
    
    public static Task NotFound(this IReaperEndpoint endpoint)
    {
        return endpoint.StatusCode(StatusCodes.Status404NotFound);
    }
    
    public static async Task NotFound<T>(this IReaperEndpoint endpoint, T? result = default)
    {
        await endpoint.NotFound();
        await ExecuteResponseAsync(endpoint.Context, result);
    }
    
    public static Task InternalServerError(this IReaperEndpoint endpoint)
    {
        return endpoint.StatusCode(StatusCodes.Status500InternalServerError);
    }
    
    public static async Task InternalServerError<T>(this IReaperEndpoint endpoint, T? result = default)
    {
        await endpoint.InternalServerError();
        await ExecuteResponseAsync(endpoint.Context, result);
    }
    
    public static Task NoContent(this IReaperEndpoint endpoint)
    {
        return endpoint.StatusCode(StatusCodes.Status204NoContent);
    }
    
    public static Task Created(this IReaperEndpoint endpoint)
    {
        return endpoint.StatusCode(StatusCodes.Status201Created);
    }
    
    public static async Task Created<T>(this IReaperEndpoint endpoint, T? result = default)
    {
        await endpoint.Created();
        await ExecuteResponseAsync(endpoint.Context, result);
    }

    private static async Task ExecuteResponseAsync<T>(HttpContext httpContext, T? response = default)
    {
        if (response == null)
        {
            await httpContext.Response.CompleteAsync();
            return;
        }
        if (response is IResult r)
        {
            await r.ExecuteAsync(httpContext);
        }
        else if (response is string s)
        {
            await httpContext.Response.WriteAsync(s);
        }
        else
        {
            var jsonOptions = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;
            var jsonTypeInfo = (JsonTypeInfo<T?>)jsonOptions.SerializerOptions.GetTypeInfo(typeof(T));
            await ResponseHelpers.WriteJsonResponseAsync(httpContext.Response, response, jsonTypeInfo);
        }
    }
}