namespace Reaper.Validation.Context;

public class ReaperValidationContext : IReaperValidationContext
{
    public RequestValidationFailureType FailureType { get; set; }
}