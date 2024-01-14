using Microsoft.AspNetCore.Http;

namespace Reaper.Response;

// TODO Confirm
public class ReaperResponse : IResult
{
    public ReaperResponse() { }

    public ReaperResponse(int statusCode)
    {
        statusCode = this.statusCode;
    }
    
    public ReaperResponse(int statusCode, string contentType)
    {
        statusCode = this.statusCode;
        contentType = this.contentType;
    }

    private readonly int statusCode = StatusCodes.Status200OK;
    private readonly string contentType = "text/plain";
    
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof (httpContext));
        
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = contentType;

        //await httpContext.Response.WriteAsJsonAsync("Hello World!");
    }
}