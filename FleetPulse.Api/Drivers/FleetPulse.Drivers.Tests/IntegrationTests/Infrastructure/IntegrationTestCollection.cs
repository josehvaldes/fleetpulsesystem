using Xunit;

namespace FleetPulse.Drivers.Tests.IntegrationTests.Infrastructure
{
    [CollectionDefinition("Integration")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
