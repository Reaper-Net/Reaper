using FluentValidation;
using Reaper.Validation;

namespace Reaper.TestWeb.Endpoints.ReaperEndpointRX;

[ReaperRoute(HttpVerbs.Post, "/rerx/validator")]
public class ValidatorWriteEndpoint : ReaperEndpointRX<ValidatorWriteRequest>
{
    public override Task ExecuteAsync(ValidatorWriteRequest request)
    {
        return Context.Response.WriteAsync(request.Message!);
    }
}

public class ValidatorWriteRequest
{
    public string? Message { get; set; }
}

public class ValidatorWriteRequestValidator : RequestValidator<ValidatorWriteRequest>
{
    public ValidatorWriteRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty();
    }
}