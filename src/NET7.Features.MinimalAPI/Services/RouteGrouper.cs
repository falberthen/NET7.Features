namespace NET7.Features.MinimalAPI.Services;

public static class RouteGrouper
{
    public static RouteGroupBuilder MapGitHubIssuesRoutes(this RouteGroupBuilder group,
        IServiceCollection services)
    {
        var handler = services.BuildServiceProvider()
            .GetRequiredService<IGithubIssuesRouteHandler>();

        group.MapGet(Routes.ListIssues, handler.HandleList)
            .AddEndpointFilter(async (efiContext, next) =>
            {
                if (efiContext.Arguments[0] is null)
                    return await next(efiContext);

                // If the optional route parameter is set, validate it
                var maxValue = efiContext.GetArgument<int>(0);
                if (maxValue <= 0)
                    return Results.BadRequest($"The max range must be higher than zero.");

                return await next(efiContext);
            }).CacheOutput(PolicyNames.TwentySecondsCachePolicy); // Added TwentySecondsCachePolicy

        group.MapGet(Routes.GetById, handler.HandleGetById);            

        group.MapPost(Routes.AddIssue, handler.HandleAdd)            
            .AddEndpointFilter<ValidationFilter<GithubIssue>>(); 

        return group;
    }
}
