using Microsoft.AspNetCore.Http;
using Reaper;
using Reaper.Context;
using Reaper.Validation;

namespace UnitTests.Resources;

public class EndpointHarness : IReaperEndpoint
{
    public void SetContextProvider(IReaperExecutionContextProvider provider)
    {
        throw new NotImplementedException();
    }

    public IReaperExecutionContext ReaperExecutionContext { get; } = default!;
    public HttpContext Context { get; } = new DefaultHttpContext();
    public HttpRequest Request => Context.Request;
    public HttpResponse Response => Context.Response;
    public IReaperValidationContext ValidationContext { get; } = default!;
}