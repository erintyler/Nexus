using Xunit;

namespace Nexus.Api.IntegrationTests;

/// <summary>
/// Collection definition for Aspire integration tests.
/// This ensures that tests using the AspireAppHostFixture don't run in parallel,
/// which could cause port conflicts and resource contention.
/// </summary>
[CollectionDefinition("Aspire")]
public class AspireCollection : ICollectionFixture<AspireAppHostFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
