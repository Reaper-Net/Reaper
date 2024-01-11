namespace Reaper.Validation;

public interface IReaperValidationContext
{
    RequestValidationFailureType FailureType { get; set; }
}