namespace NET7.Features.MinimalAPI.Tests;

public class AuthenticatedUserPolicyTests : IntegrationTestBase
{
    public AuthenticatedUserPolicyTests(WebApplicationFactory<Program> appFactory)
        : base(appFactory) {}

    [Fact]
    public async Task ListIssues_WhenUnderAuthenticatedUserPolicy_0out50RequestsShouldBeRejected()
    {
        // Arrange        
        var numberOfRequests = 50;
        var results = new List<HttpResponseMessage>();
        var scheme = "TestScheme";

        using var client = CreateClient(services =>
            services
                .AddAuthentication(defaultScheme: scheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    scheme, options => {}
                )
            );

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            scheme: scheme);

        var route = BuildFullRoute(Routes.ListIssues);

        // Act
        for (int i = 0; i < numberOfRequests; i++)
            results.Add(await client.GetAsync(route));

        // Assert
        AssertStatusCodeResponses(results, HttpStatusCode.TooManyRequests, expectedCount: 0);
        AssertStatusCodeResponses(results, HttpStatusCode.OK, expectedCount: 50);
    }

    [Fact]
    public async Task ListIssues_WhenUnderAuthenticatedUserPolicy_10out50RequestsShouldBeRejected()
    {
        // Arrange
        var numberOfRequests = 50;
        var results = new List<HttpResponseMessage>();

        using var client = CreateClient(services => { });
        var route = BuildFullRoute(Routes.ListIssues);

        // Act                
        for (int i = 0; i < numberOfRequests; i++) // limited requests for non-authenticated users
            results.Add(await client.GetAsync(route));

        // Assert
        AssertStatusCodeResponses(results, HttpStatusCode.TooManyRequests, expectedCount: 10);
        AssertStatusCodeResponses(results, HttpStatusCode.OK, expectedCount: 40);
    }
}