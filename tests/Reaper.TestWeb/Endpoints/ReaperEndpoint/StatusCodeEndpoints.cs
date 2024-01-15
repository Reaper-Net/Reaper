using Reaper.Response;

namespace Reaper.TestWeb.Endpoints.ReaperEndpoint;

[ReaperRoute(HttpVerbs.Get, "/re/200")]
public class Status200Endpoint : Reaper.ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return this.Ok();
    }
}

[ReaperRoute(HttpVerbs.Get, "/re/400")]
public class Status400Endpoint : Reaper.ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return this.BadRequest();
    }
}

[ReaperRoute(HttpVerbs.Get, "/re/404")]
public class Status404Endpoint : Reaper.ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return this.NotFound();
    }
}

[ReaperRoute(HttpVerbs.Get, "/re/w200")]
public class Status200WriterEndpoint : Reaper.ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return this.Ok("Hello, World!");
    }
}

[ReaperRoute(HttpVerbs.Get, "/re/w400")]
public class Status400WriterEndpoint : Reaper.ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return this.BadRequest("Hello, World!");
    }
}

[ReaperRoute(HttpVerbs.Get, "/re/w404")]
public class Status404WriterEndpoint : Reaper.ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return this.NotFound("Hello, World!");
    }
}

[ReaperRoute(HttpVerbs.Get, "/re/j200")]
public class Status200JsonWriterEndpoint : Reaper.ReaperEndpointXR<SampleResponse>
{
    public override async Task ExecuteAsync()
    {
        await this.Ok(new SampleResponse
        {
            Message = "Hello, World!"
        });
    }
}

public class SampleResponse
{
    public string Message { get; set; }
}
