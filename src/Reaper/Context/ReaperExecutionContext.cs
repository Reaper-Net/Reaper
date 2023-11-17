using Microsoft.AspNetCore.Http;

namespace Reaper.Context;

public class ReaperExecutionContext
{
    public HttpContext HttpContext { get; init; } = default!;
    
    public string RequestTraceIdentifier => HttpContext.TraceIdentifier;
    
}