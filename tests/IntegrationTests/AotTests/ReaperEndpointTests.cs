namespace IntegrationTests.AotTests;

[Collection("AOT")]
public class ReaperEndpointTests(AotTestFixture fixture) : Tests.ReaperEndpointTests(fixture.Client) { }