using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Reaper.Handlers;
using Reaper.Validation;
using Reaper.Validation.Context;

namespace Reaper;

public static class WebApplicationBuilderExtensions
{
    public static void UseReaperValidation(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<JsonOptions>, ReaperProblemDetailsJsonOptionsSetup>());
        builder.Services.TryAddTransient<IReaperValidationContext, FluentValidationContext>();
        builder.Services.TryAddScoped<IValidationFailureHandler, FluentValidationFailureHandler>();
    }
}