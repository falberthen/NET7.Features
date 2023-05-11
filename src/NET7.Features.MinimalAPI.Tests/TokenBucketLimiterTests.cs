namespace NET7.Features.MinimalAPI.Tests;

public class TokenBucketLimiterTests : IntegrationTestBase
{
    public TokenBucketLimiterTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task ListIssues_WhenTokenBucketLimitOf20Requests_5out25RequestsShouldBeRejected()
    {
        // Arrange
        var numberOfRequests = 25;
        var bucketTokenLimit = 20;
        var tokensToRestorePerPeriod = 10;
        var replenishmentPeriod = TimeSpan.FromMinutes(1);
        var results = new List<HttpResponseMessage>();

        using var client = CreateClient(services =>
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: GetPartitionKey(httpContext),
                        factory: partition => new TokenBucketRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            TokenLimit = bucketTokenLimit,
                            ReplenishmentPeriod = replenishmentPeriod,
                            TokensPerPeriod = tokensToRestorePerPeriod,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        }
                    )
                );
                options.OnRejected = async (context, token) =>
                    await HandleRateLimiterRejectionAsync(context, token);
            })
        );

        var route = BuildFullRoute(Routes.ListIssues);

        // Act        
        for (int i = 0; i < numberOfRequests; i++)
            results.Add(await client.GetAsync(route));

        // Assert
        AssertStatusCodeResponses(results, HttpStatusCode.TooManyRequests, expectedCount: 5);
        AssertStatusCodeResponses(results, HttpStatusCode.OK, expectedCount: 20);
    }
}