using Microsoft.AspNetCore.Http;
using Reaper.Validation;

namespace Reaper.Context;

public class ReaperExecutionContext : IReaperExecutionContext
{
    public HttpContext HttpContext { get; private set; } = default!;

    public IReaperValidationContext ValidationContext { get; private set; } = default!;
    
    public string RequestTraceIdentifier => HttpContext.TraceIdentifier;
    
    public void TrySetDefaultContexts(HttpContext httpContext, IReaperValidationContext validationContext)
    {
        if (HttpContext != default! || ValidationContext != default!)
        {
            return;
        }
        
        HttpContext = httpContext;
        ValidationContext = validationContext;
    }
}