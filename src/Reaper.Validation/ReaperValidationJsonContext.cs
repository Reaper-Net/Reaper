using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using ValidationProblemDetails = Reaper.Validation.Responses.ValidationProblemDetails;

namespace Reaper.Validation;

[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]
internal partial class ReaperValidationJsonContext : JsonSerializerContext
{
    
}

internal sealed class ReaperProblemDetailsJsonOptionsSetup : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        // Always insert the ProblemDetailsJsonContext to the beginning of the chain at the time
        // this Configure is invoked. This JsonTypeInfoResolver will be before the default reflection-based resolver,
        // and before any other resolvers currently added.
        // If apps need to customize ProblemDetails serialization, they can prepend a custom ProblemDetails resolver
        // to the chain in an IConfigureOptions<JsonOptions> registered after the call to AddProblemDetails().
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, new ReaperValidationJsonContext());
    }
}
