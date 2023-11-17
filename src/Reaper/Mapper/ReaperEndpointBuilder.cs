namespace Reaper.Mapper;

public class ReaperEndpointBuilder<TReaperEndpoint>
    where TReaperEndpoint: ReaperEndpointBase
{
    public string Route { get; private set; } = default!;
    public string Verb { get; private set; } = default!;
    public bool ReaperHandler { get; private set; }
    
    public Type EndpointType => typeof(TReaperEndpoint);
    public Type? RequestType { get; private set; }
    public Type? ResponseType { get; private set; }
    
    public ReaperEndpointBuilder<TReaperEndpoint> WithRoute(string route)
    {
        Route = route;
        return this;
    }
    
    public ReaperEndpointBuilder<TReaperEndpoint> WithVerb(string verb)
    {
        Verb = verb;
        return this;
    }
    
    public ReaperEndpointBuilder<TReaperEndpoint> WithReaperHandler()
    {
        ReaperHandler = true;
        return this;
    }
    
    public ReaperEndpointBuilder<TReaperEndpoint> WithRequest<TRequest>()
    {
        RequestType = typeof(TRequest);
        return this;
    }
    
    public ReaperEndpointBuilder<TReaperEndpoint> WithResponse<TResponse>()
    {
        ResponseType = typeof(TResponse);
        return this;
    }
}