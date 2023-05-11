namespace NET7.Features.MinimalAPI.Tests;

public class ConcurrencyLimiterTests : IntegrationTestBase
{
    public ConcurrencyLimiterTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]    
    public async Task ListIssues_WhenConcurrencyLimitOf2Requests_8out10RequestsShouldBeRejected()
    {
        // Arrange
        var numberOfRequests = 10;
        var permitLimit = 2; // Only two requests at the time

        using var client = CreateClient(services =>
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetConcurrencyLimiter(
                        partitionKey: GetPartitionKey(httpContext),
                        factory: partition => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = permitLimit
                        }
                    )
                );
                options.OnRejected = async (context, token) =>
                    await HandleRateLimiterRejectionAsync(context, token);
            })
        );

        var route = BuildFullRoute(Routes.ListIssues);
        var apiCalls = Enumerable.Range(0, numberOfRequests)
            .Select(_ => client.GetAsync(route));

        // Act
        var results = await Task.WhenAll(apiCalls); // concurrent requests

        // Assert
        AssertStatusCodeResponses(results, HttpStatusCode.TooManyRequests, expectedCount: 8);
        AssertStatusCodeResponses(results, HttpStatusCode.OK, expectedCount: 2);
    }
}