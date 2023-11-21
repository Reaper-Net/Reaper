using Microsoft.AspNetCore.Http;

namespace Reaper.EndpointMapper;

public abstract record ReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse>(string Route, string Verb)
    where TEndpoint: ReaperEndpointBase 
{
}

public record DirectReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse>(string Route, string Verb)
    : ReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse>(Route, Verb)
    where TEndpoint: ReaperEndpointBase
{
    public required Delegate Handler { get; init; }
    
    public required Func<Delegate, RequestDelegateFactoryOptions, RequestDelegateMetadataResult?, RequestDelegateResult> RequestDelegateGenerator { get; init; }
    
    public bool IsJsonRequest { get; set; }
}

public record HandlerReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse>(string Route, string Verb)
    : ReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse>(Route, Verb)
    where TEndpoint: ReaperEndpointBase
{
    
}

public struct EmptyRequestResponse { }