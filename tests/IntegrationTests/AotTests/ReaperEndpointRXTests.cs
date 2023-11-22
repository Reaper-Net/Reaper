namespace IntegrationTests.AotTests;

[Collection("AOT")]
// ReSharper disable once InconsistentNaming
public class ReaperEndpointRXTests(AotTestFixture fixture) : Tests.ReaperEndpointRXTests(fixture.Client) { }