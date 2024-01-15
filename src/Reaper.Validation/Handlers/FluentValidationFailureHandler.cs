using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reaper.Context;
using Reaper.RequestDelegateSupport;
using Reaper.Validation;
using Reaper.Validation.Context;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using ValidationProblemDetails = Reaper.Validation.Responses.ValidationProblemDetails;

namespace Reaper.Handlers;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class FluentValidationFailureHandler(IReaperExecutionContextProvider reaperExecutionContextProvider) : IValidationFailureHandler
{
    private readonly IReaperExecutionContext reaperExecutionContext = reaperExecutionContextProvider.Context;

    public virtual async Task HandleValidationFailure()
    {
        if (reaperExecutionContext.ValidationContext is not FluentValidationContext validationContext)
        {
            throw new InvalidOperationException("FluentValidationContext is not available.");
        }

        var httpContext = reaperExecutionContext.HttpContext;
        var jsonOptions = httpContext.RequestServices.GetService<IOptions<JsonOptions>>()!.Value;
        var jsonTypeInfo = (JsonTypeInfo<ProblemDetails?>)jsonOptions.SerializerOptions.GetTypeInfo(typeof(ProblemDetails));
        var validationJsonTypeInfo = (JsonTypeInfo<ValidationProblemDetails?>)jsonOptions.SerializerOptions.GetTypeInfo(typeof(ValidationProblemDetails));
        
        switch (validationContext.FailureType)
        {
            case RequestValidationFailureType.None:
                // No action, we shouldn't be here.
                return;
            case RequestValidationFailureType.BodyRequiredNotProvided:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await ResponseHelpers.ExecuteReturnAsync(new ProblemDetails()
                {
                    Type = "https://reaper.divergent.dev/validation/body-required",
                    Title = "Request body is required.",
                }, httpContext, jsonTypeInfo, "application/problem+json");
                break;
            case RequestValidationFailureType.UserDefinedValidationFailure:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                var validationFailures = validationContext.ValidationResult!.Errors.GroupBy(m => m.PropertyName)
                    .ToDictionary(m => m.Key, m => string.Join(", ", m.Select(n => n.ErrorMessage)));
                await ResponseHelpers.ExecuteReturnAsync(new ValidationProblemDetails
                {
                    Type = "https://reaper.divergent.dev/validation/failure",
                    Title = "Validation failed for this request.",
                    ValidationFailures = validationFailures
                }, httpContext, validationJsonTypeInfo, "application/problem+json");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}