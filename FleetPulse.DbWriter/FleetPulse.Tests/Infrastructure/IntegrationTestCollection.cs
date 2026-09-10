
using Xunit;

namespace FleetPulse.Tests.Infrastructure
{
    [CollectionDefinition("Integration")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
