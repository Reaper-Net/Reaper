using FluentValidation;

namespace Reaper.Validation;

public abstract class RequestValidator<TRequest> : AbstractValidator<TRequest>
{
}