namespace IntegrationTests.WafTests;

[Collection("WAF")]
// ReSharper disable once InconsistentNaming
public class ReaperEndpointXRTests(WafTextFixture fixture) : Tests.ReaperEndpointXRTests(fixture.Client) { }