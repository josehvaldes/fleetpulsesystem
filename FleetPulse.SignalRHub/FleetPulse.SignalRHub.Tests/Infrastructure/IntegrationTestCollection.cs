using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FleetPulse.SignalRHub.Tests.Infrastructure
{
    [CollectionDefinition("Integration")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
