namespace Reaper.Handlers;

public interface IValidationFailureHandler
{
    Task HandleValidationFailure();
}