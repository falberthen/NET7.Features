namespace NET7.Features.MinimalAPI.Tests.Base;

public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    protected HttpClient CreateClient(Action<IServiceCollection> services)
    {
        return _factory
            .WithWebHostBuilder(builder =>
                builder.UseEnvironment("Development")
                .ConfigureServices(services))
                .CreateClient();
    }

    protected async Task HandleRateLimiterRejectionAsync(OnRejectedContext context, 
        CancellationToken token)
    {
        var errorMessage = "Too many requests. Please try again";
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            errorMessage = $"{errorMessage} after {retryAfter.TotalMinutes} minute(s). ";

        await context.HttpContext.Response
            .WriteAsync(errorMessage, cancellationToken: token);
    }

    protected void AssertStatusCodeResponses(IEnumerable<HttpResponseMessage> responses, 
        HttpStatusCode expectedStatusCode, int expectedCount)
    {
        var filteredResponses = responses.Where(r => r.StatusCode == expectedStatusCode).ToList();
        Assert.Equal(expectedCount, filteredResponses.Count);
        Assert.All(filteredResponses, r => Assert.Equal(expectedStatusCode, r.StatusCode));
    }

    protected string GetPartitionKey(HttpContext httpContext) =>
        httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString();

    public string BuildFullRoute(string route) =>
        Path.Combine(Routes.BaseRoute.TrimEnd('/'), route.TrimStart('/'));
}