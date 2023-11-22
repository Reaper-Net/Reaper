namespace IntegrationTests.WafTests;

[Collection("WAF")]
// ReSharper disable once InconsistentNaming
public class ReaperEndpointRRTests(WafTextFixture fixture) : Tests.ReaperEndpointRRTests(fixture.Client) { }