namespace Reaper.Validation;

public enum RequestValidationFailureType
{
    None,
    BodyRequiredNotProvided,
    UserDefinedValidationFailure
}