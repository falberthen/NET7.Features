namespace NET7.Features.MinimalAPI.Tests;

public class SlidingWindowLimiterTests : IntegrationTestBase
{
    public SlidingWindowLimiterTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]    
    public async Task ListIssues_WhenSlidingWindowLimitOf10Requests_10out20RequestsShouldBeRejected()
    {
        // Arrange        
        var numberOfRequests = 20;
        var permitLimit = 10; // maximum of requests per segment
        var window = TimeSpan.FromSeconds(30); // 30 seconds window
        var segmentsPerWindow = 3;
        var results = new List<HttpResponseMessage>();        

        // Segments sliding interval = (30 seconds / 3) equals 3 segments of 10 seconds

        using var client = CreateClient(services =>
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: GetPartitionKey(httpContext),
                        factory: partition => new SlidingWindowRateLimiterOptions
                        {
                            Window = window,
                            PermitLimit = permitLimit,
                            SegmentsPerWindow = segmentsPerWindow,
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
        AssertStatusCodeResponses(results, HttpStatusCode.TooManyRequests, expectedCount: 10);
        AssertStatusCodeResponses(results, HttpStatusCode.OK, expectedCount: 10);
    }
}