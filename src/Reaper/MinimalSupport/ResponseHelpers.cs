using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;

namespace Reaper.MinimalSupport;

public static class ResponseHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Task ExecuteReturnAsync<T>(object? obj, HttpContext httpContext, JsonTypeInfo<T?> jsonTypeInfo)
    {
        if (obj is IResult r)
        {
            return r.ExecuteAsync(httpContext);
        }
        else if (obj is string s)
        {
            return httpContext.Response.WriteAsync(s);
        }
        else
        {
            return WriteJsonResponseAsync(httpContext.Response, (T?)obj, jsonTypeInfo);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed ASP.NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private static Task WriteJsonResponseAsync<T>(HttpResponse response, T? value, JsonTypeInfo<T?> jsonTypeInfo)
    {
        var runtimeType = value?.GetType();

        if (jsonTypeInfo.ShouldUseWith(runtimeType))
        {
            return HttpResponseJsonExtensions.WriteAsJsonAsync(response, value, jsonTypeInfo, default);
        }

        return response.WriteAsJsonAsync<object?>(value, jsonTypeInfo.Options);
    }
    
    
    private static bool HasKnownPolymorphism(this JsonTypeInfo jsonTypeInfo)
        => jsonTypeInfo.Type.IsSealed || jsonTypeInfo.Type.IsValueType || jsonTypeInfo.PolymorphismOptions is not null;

    private static bool ShouldUseWith(this JsonTypeInfo jsonTypeInfo, [NotNullWhen(false)] Type? runtimeType)
        => runtimeType is null || jsonTypeInfo.Type == runtimeType || jsonTypeInfo.HasKnownPolymorphism();
}