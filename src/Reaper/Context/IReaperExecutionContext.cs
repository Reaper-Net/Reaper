using Microsoft.AspNetCore.Http;
using Reaper.Validation;

namespace Reaper.Context;

public interface IReaperExecutionContext
{
    HttpContext HttpContext { get; }

    IReaperValidationContext ValidationContext { get; }
    
    string RequestTraceIdentifier { get; }

    void TrySetDefaultContexts(HttpContext httpContext, IReaperValidationContext validationContext);
}