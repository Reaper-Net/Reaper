using System.Net;
using Reaper.Context;
using Reaper.Validation;

namespace Reaper.Handlers;

public class DefaultValidationFailureHandler(IReaperExecutionContextProvider reaperExecutionContextProvider) : IValidationFailureHandler
{
    private readonly IReaperExecutionContext reaperExecutionContext = reaperExecutionContextProvider.Context;

    public Task HandleValidationFailure()
    {
        if (reaperExecutionContext.ValidationContext.FailureType != RequestValidationFailureType.None)
        {
            reaperExecutionContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        return Task.CompletedTask;
    }
}