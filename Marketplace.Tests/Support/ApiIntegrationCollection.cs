using Xunit;

namespace Marketplace.Tests.Support;

[CollectionDefinition("ApiIntegration", DisableParallelization = true)]
public class ApiIntegrationCollection : ICollectionFixture<ApiWebApplicationFactory>
{
}
