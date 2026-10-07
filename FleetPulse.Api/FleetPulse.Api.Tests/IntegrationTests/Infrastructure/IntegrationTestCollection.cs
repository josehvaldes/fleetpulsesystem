using Xunit;

namespace FleetPulse.Api.Tests.IntegrationTests.Infrastructure
{
    [CollectionDefinition("Integration")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
