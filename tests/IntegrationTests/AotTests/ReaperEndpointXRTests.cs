namespace IntegrationTests.AotTests;

[Collection("AOT")]
// ReSharper disable once InconsistentNaming
public class ReaperEndpointXRTests(AotTestFixture fixture) : Tests.ReaperEndpointXRTests(fixture.Client) { }