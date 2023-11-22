namespace IntegrationTests.AotTests;

[Collection("AOT")]
// ReSharper disable once InconsistentNaming
public class ReaperEndpointRRTests(AotTestFixture fixture) : Tests.ReaperEndpointRRTests(fixture.Client) { }