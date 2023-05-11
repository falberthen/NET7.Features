namespace NET7.Features.MinimalAPI.Policies;

public class AuthenticatedUserPolicy : IRateLimiterPolicy<string>
{
    public static readonly AuthenticatedUserPolicy Instance = new();

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var nonAuthPermitLimit = 40; // 40 requests per minute
        var authenticatedPermitLimit = 400; // 400 requests per minute
        var window = TimeSpan.FromMinutes(1);

        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;

        // Authenticated requests
        if (isAuthenticated)
        {
            var identityName = httpContext.User.Identity?.Name!.ToString();

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: identityName!,
                partition => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authenticatedPermitLimit,
                    Window = window
                }
            );
        }

        // Non-authenticated requests
        return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Request.Headers.Host.ToString(),
                partition => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = nonAuthPermitLimit,
                    Window = window
                }
            );
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected
    {
        get => (context, lease) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return new ValueTask();
        };
    }
}