namespace NET7.Features.MinimalAPI.Services;

public class GithubIssuesRouteHandler : IGithubIssuesRouteHandler
{
    private List<GithubIssue> _issues = IssuesBuilder
        .BuildIssues(20).ToList();    

    public async Task<Results<Ok<GithubIssue>, NotFound<string>>> HandleGetById(string id) // multiple results 
    {
        await DelayRequest();

        var issue = _issues.Concat(_issues)
            .Where(i => i.Id == id)
            .FirstOrDefault();

        if(issue is null)
            return TypedResults.NotFound($"Issue with Id {id} was not found.");

        return TypedResults.Ok(issue);
    }

    public async Task<Ok<List<GithubIssue>>> HandleList(int? take = null)
    {
        await DelayRequest();
        
        int defaultTake = _issues.Count();
        take ??= defaultTake;
        
        return TypedResults.Ok(
            _issues.Take(take.Value).ToList()
        );
    }

    public async Task<Created<GithubIssue>> HandleAdd([FromBody] GithubIssue githubIssue)
    {
        await DelayRequest();

        _issues.Add(githubIssue);
        return TypedResults
            .Created($"{Routes.BaseRoute}/{githubIssue.Id}", githubIssue);
    }

    private async Task DelayRequest() =>
        await Task.Delay(50);
}