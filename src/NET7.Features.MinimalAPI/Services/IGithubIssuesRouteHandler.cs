namespace NET7.Features.MinimalAPI.Services;

public interface IGithubIssuesRouteHandler
{
    Task<Results<Ok<GithubIssue>, NotFound<string>>> HandleGetById(string id);
    Task<Ok<List<GithubIssue>>> HandleList(int? take = null);
    Task<Created<GithubIssue>> HandleAdd(GithubIssue githubIssue);
}