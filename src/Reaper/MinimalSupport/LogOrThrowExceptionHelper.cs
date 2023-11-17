using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Reaper.MinimalSupport;

public sealed class LogOrThrowExceptionHelper
{
    private readonly ILogger? _rdgLogger;
    private readonly bool _shouldThrow;

    public LogOrThrowExceptionHelper(IServiceProvider? serviceProvider, RequestDelegateFactoryOptions? options)
    {
        var loggerFactory = serviceProvider?.GetRequiredService<ILoggerFactory>();
        _rdgLogger = loggerFactory?.CreateLogger("Microsoft.AspNetCore.Http.RequestDelegateGenerator.RequestDelegateGenerator");
        _shouldThrow = options?.ThrowOnBadRequest ?? false;
    }

    public void RequestBodyIOException(IOException exception)
    {
        if (_rdgLogger != null)
        {
            _requestBodyIOException(_rdgLogger, exception);
        }
    }

    private static readonly Action<ILogger, Exception?> _requestBodyIOException =
        LoggerMessage.Define(LogLevel.Debug, new EventId(1, "RequestBodyIOException"), "Reading the request body failed with an IOException.");

    public void InvalidJsonRequestBody(string parameterTypeName, string parameterName, Exception exception)
    {
        if (_shouldThrow)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Failed to read parameter \"{0} {1}\" from the request body as JSON.", parameterTypeName, parameterName);
            throw new BadHttpRequestException(message, exception);
        }

        if (_rdgLogger != null)
        {
            _invalidJsonRequestBody(_rdgLogger, parameterTypeName, parameterName, exception);
        }
    }

    private static readonly Action<ILogger, string, string, Exception?> _invalidJsonRequestBody =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(2, "InvalidJsonRequestBody"), "Failed to read parameter \"{ParameterType} {ParameterName}\" from the request body as JSON.");

    public void ParameterBindingFailed(string parameterTypeName, string parameterName, string sourceValue)
    {
        if (_shouldThrow)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Failed to bind parameter \"{0} {1}\" from \"{2}\".", parameterTypeName, parameterName, sourceValue);
            throw new BadHttpRequestException(message);
        }

        if (_rdgLogger != null)
        {
            _parameterBindingFailed(_rdgLogger, parameterTypeName, parameterName, sourceValue, null);
        }
    }

    private static readonly Action<ILogger, string, string, string, Exception?> _parameterBindingFailed =
        LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId(3, "ParameterBindingFailed"), "Failed to bind parameter \"{ParameterType} {ParameterName}\" from \"{SourceValue}\".");

    public void RequiredParameterNotProvided(string parameterTypeName, string parameterName, string source)
    {
        if (_shouldThrow)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Required parameter \"{0} {1}\" was not provided from {2}.", parameterTypeName, parameterName, source);
            throw new BadHttpRequestException(message);
        }

        if (_rdgLogger != null)
        {
            _requiredParameterNotProvided(_rdgLogger, parameterTypeName, parameterName, source, null);
        }
    }

    private static readonly Action<ILogger, string, string, string, Exception?> _requiredParameterNotProvided =
        LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId(4, "RequiredParameterNotProvided"), "Required parameter \"{ParameterType} {ParameterName}\" was not provided from {Source}.");

    public void ImplicitBodyNotProvided(string parameterName)
    {
        if (_shouldThrow)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Implicit body inferred for parameter \"{0}\" but no body was provided. Did you mean to use a Service instead?", parameterName);
            throw new BadHttpRequestException(message);
        }

        if (_rdgLogger != null)
        {
            _implicitBodyNotProvided(_rdgLogger, parameterName, null);
        }
    }

    private static readonly Action<ILogger, string, Exception?> _implicitBodyNotProvided =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(5, "ImplicitBodyNotProvided"), "Implicit body inferred for parameter \"{ParameterName}\" but no body was provided. Did you mean to use a Service instead?");

    public void UnexpectedJsonContentType(string? contentType)
    {
        if (_shouldThrow)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Expected a supported JSON media type but got \"{0}\".", contentType);
            throw new BadHttpRequestException(message, StatusCodes.Status415UnsupportedMediaType);
        }

        if (_rdgLogger != null)
        {
            _unexpectedJsonContentType(_rdgLogger, contentType ?? "(none)", null);
        }
    }

    private static readonly Action<ILogger, string, Exception?> _unexpectedJsonContentType =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(6, "UnexpectedContentType"), "Expected a supported JSON media type but got \"{ContentType}\".");

    public void UnexpectedNonFormContentType(string? contentType)
    {
        if (_shouldThrow)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Expected a supported form media type but got \"{0}\".", contentType);
            throw new BadHttpRequestException(message, StatusCodes.Status415UnsupportedMediaType);
        }

        if (_rdgLogger != null)
        {
            _unexpectedNonFormContentType(_rdgLogger, contentType ?? "(none)", null);
        }
    }

    private static readonly Action<ILogger, string, Exception?> _unexpectedNonFormContentType =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(7, "UnexpectedNonFormContentType"), "Expected a supported form media type but got \"{ContentType}\".");

    public void InvalidFormRequestBody(string parameterTypeName, string parameterName, Exception exception)
    {
        if (_shouldThrow)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Failed to read parameter \"{0} {1}\" from the request body as form.", parameterTypeName, parameterName);
            throw new BadHttpRequestException(message, exception);
        }

        if (_rdgLogger != null)
        {
            _invalidFormRequestBody(_rdgLogger, parameterTypeName, parameterName, exception);
        }
    }

    private static readonly Action<ILogger, string, string, Exception?> _invalidFormRequestBody =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(8, "InvalidFormRequestBody"), "Failed to read parameter \"{ParameterType} {ParameterName}\" from the request body as form.");
}