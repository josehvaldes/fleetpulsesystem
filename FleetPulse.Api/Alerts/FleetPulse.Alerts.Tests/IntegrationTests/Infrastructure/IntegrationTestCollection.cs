using Xunit;

namespace FleetPulse.Alerts.Tests.IntegrationTests.Infrastructure
{
    [CollectionDefinition("Integration")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
