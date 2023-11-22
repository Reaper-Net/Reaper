using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reaper.Context;

namespace Reaper;

public static class WebApplicationBuilderExtensions
{
    public static void UseReaper(this WebApplicationBuilder builder)
    {
        throw new InvalidProgramException("Reaper Source Generator Interceptors not operative.");
    }
}