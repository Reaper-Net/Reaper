using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reaper.Middleware;

namespace Reaper;

// To be intercepted
public static class WebApplicationExtensions
{
    public static void MapReaperEndpoints(this WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Reaper");
        
        logger.LogCritical("💀❌ Reaper is not working correctly. This call should have been intercepted. Is the source generator running?");
        throw new InvalidProgramException("Reaper Source Generator Interceptor not operative.");
    }

    public static WebApplication UseReaperMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ReaperExecutionContextMiddleware>();
        return app;
    }
}