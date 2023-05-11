namespace NET7.Features.MinimalAPI.Constants;

public static class Routes
{
    public const string BaseRoute = "issues";
    public const string AddIssue = "/add";
    public const string GetById = "/{id:guid}";
    public const string ListIssues = "/list";
}