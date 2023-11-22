namespace IntegrationTests.WafTests;

[Collection("WAF")]
public class ReaperEndpointTests(WafTextFixture fixture) : Tests.ReaperEndpointTests(fixture.Client) { }