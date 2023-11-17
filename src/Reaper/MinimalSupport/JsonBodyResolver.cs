using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;

namespace Reaper.MinimalSupport;

public static class JsonBodyResolver
{
    public static async ValueTask<(bool, T?)> TryResolveBodyAsync<T>(HttpContext httpContext, LogOrThrowExceptionHelper logOrThrowExceptionHelper, string parameterTypeName, string parameterName, JsonTypeInfo<T> jsonTypeInfo)
        {
            var feature = httpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpRequestBodyDetectionFeature>();
            T? bodyValue = default;
            var bodyValueSet = false;

            if (feature?.CanHaveBody == true)
            {
                if (!httpContext.Request.HasJsonContentType())
                {
                    logOrThrowExceptionHelper.UnexpectedJsonContentType(httpContext.Request.ContentType);
                    httpContext.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return (false, default);
                }
                try
                {
                    bodyValue = await httpContext.Request.ReadFromJsonAsync(jsonTypeInfo);
                    bodyValueSet = bodyValue != null;
                }
                catch (BadHttpRequestException badHttpRequestException)
                {
                    logOrThrowExceptionHelper.RequestBodyIOException(badHttpRequestException);
                    httpContext.Response.StatusCode = badHttpRequestException.StatusCode;
                    return (false, default);
                }
                catch (IOException ioException)
                {
                    logOrThrowExceptionHelper.RequestBodyIOException(ioException);
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return (false, default);
                }
                catch (System.Text.Json.JsonException jsonException)
                {
                    logOrThrowExceptionHelper.InvalidJsonRequestBody(parameterTypeName, parameterName, jsonException);
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return (false, default);
                }
            }

            if (!bodyValueSet)
            {
                logOrThrowExceptionHelper.ImplicitBodyNotProvided(parameterName);
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return (false, bodyValue);
            }

            return (true, bodyValue);
        }
}
