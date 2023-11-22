namespace IntegrationTests.WafTests;

[Collection("WAF")]
// ReSharper disable once InconsistentNaming
public class ReaperEndpointRXTests(WafTextFixture fixture) : Tests.ReaperEndpointRXTests(fixture.Client) { }