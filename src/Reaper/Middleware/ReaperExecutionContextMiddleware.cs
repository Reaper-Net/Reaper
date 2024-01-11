using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Reaper.Context;
using Reaper.Validation;

namespace Reaper.Middleware;

public class ReaperExecutionContextMiddleware(RequestDelegate next)
{
    public virtual async Task InvokeAsync(HttpContext context)
    {
        var ctxProvider = context.RequestServices.GetRequiredService<IReaperExecutionContextProvider>();
        var validationContext = context.RequestServices.GetRequiredService<IReaperValidationContext>();
        var executionContext = context.RequestServices.GetRequiredService<IReaperExecutionContext>();
        executionContext.TrySetDefaultContexts(context, validationContext);
        ctxProvider.SetContext(executionContext);
        
        await next(context);
    }
}