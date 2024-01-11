namespace Reaper.Validation;

public class ReaperValidationContext : IReaperValidationContext
{
    public RequestValidationFailureType FailureType { get; set; }
}