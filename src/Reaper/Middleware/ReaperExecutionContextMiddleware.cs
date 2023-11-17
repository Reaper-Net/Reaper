using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Reaper.Context;

namespace Reaper.Middleware;

public class ReaperExecutionContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var ctxProvider = context.RequestServices.GetRequiredService<IReaperExecutionContextProvider>();
        var reaperExecutionContext = new ReaperExecutionContext
        {
            HttpContext = context,
        };
        ctxProvider.SetContext(reaperExecutionContext);
        
        await next(context);
    }
}