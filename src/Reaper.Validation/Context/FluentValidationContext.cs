using FluentValidation.Results;

namespace Reaper.Validation.Context;

public class FluentValidationContext : IReaperValidationContext
{
    public RequestValidationFailureType FailureType { get; set; }
    
    public ValidationResult? ValidationResult { get; set; }
}