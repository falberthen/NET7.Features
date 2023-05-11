namespace NET7.Features.MinimalAPI.Tests;

public class FixedWindowLimiterTests : IntegrationTestBase
{
    public FixedWindowLimiterTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task ListIssues_WhenFixedWindowLimitOf10RequestsPerMinute_5out15RequestsShouldBeRejected()
    {
        // Arrange
        var numberOfRequests = 15;
        var permitLimit = 10;
        var timeWindow = TimeSpan.FromMinutes(1);
        var results = new List<HttpResponseMessage>();

        using var client = CreateClient(services =>
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetPartitionKey(httpContext),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = timeWindow
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
        AssertStatusCodeResponses(results, HttpStatusCode.OK, expectedCount: 10);
    }

    [Fact]
    public async Task ListIssues_WhenQueueingFixedWindowLimitOf10RequestsPer10Sec_0out15RequestsShouldBeRejected()
    {
        // Arrange
        var numberOfRequests = 15;
        var permitLimit = 10;
        var queueLimit = 5;
        var timeWindow = TimeSpan.FromSeconds(10);
        var results = new List<HttpResponseMessage>();

        using var client = CreateClient(services =>
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetPartitionKey(httpContext),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = permitLimit,
                            QueueLimit = queueLimit,
                            Window = timeWindow,
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
        AssertStatusCodeResponses(results, HttpStatusCode.TooManyRequests, expectedCount: 0);
        AssertStatusCodeResponses(results, HttpStatusCode.OK, expectedCount: 15);
    }

    [Fact]
    public async Task ListIssues_WhenChainedFixedWindowLimitOf60RequestsPerMinute_10out20RequestsShouldBeRejected()
    {
        // Arrange
        var numberOfRequests = 20;
        var initialPermitLimit = 10;
        var initialTimeWindow = TimeSpan.FromSeconds(10);

        var totalPermitLimit = 60;
        var totalTimeWindow = TimeSpan.FromMinutes(1);
        var results = new List<HttpResponseMessage>();

        using var client = CreateClient(services =>
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: GetPartitionKey(httpContext),
                            factory => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = initialPermitLimit,
                                Window = initialTimeWindow,
                            })
                    ),
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: GetPartitionKey(httpContext),
                            factory => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = totalPermitLimit,
                                Window = totalTimeWindow
                            })
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