var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddValidatorsFromAssemblyContaining<GithubIssueValidator>();
services.AddScoped<IGithubIssuesRouteHandler, GithubIssuesRouteHandler>();

// Adding rate limiter and policy
services.AddRateLimiter(options =>
{
    options.AddPolicy(PolicyNames.AuthenticatedUserPolicy, AuthenticatedUserPolicy.Instance);
});

// Added cache policy
builder.Services.AddOutputCache((opts) => {
    opts.AddPolicy(PolicyNames.TwentySecondsCachePolicy, (policyBuilder) =>
        policyBuilder.Expire(TimeSpan.FromSeconds(20)));
});

var app = builder.Build();

// Using rate limiter
app.UseRateLimiter();

// Using Output cache
app.UseOutputCache();

// Grouping routes
app.MapGroup(Routes.BaseRoute)
    .MapGitHubIssuesRoutes(services)
    .RequireRateLimiting(PolicyNames.AuthenticatedUserPolicy); // Requiring the AuthenticatedUserPolicy

app.Run();

public partial class Program { }