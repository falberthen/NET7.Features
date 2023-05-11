namespace NET7.Features.MinimalAPI.Services;

public static class IssuesBuilder
{
    public static IEnumerable<GithubIssue> BuildIssues(int? maxIssues = null)
    {
        const int DefaultMaxIssues = 100;
        maxIssues ??= DefaultMaxIssues;
        Random random = new();

        return Enumerable.Range(1, maxIssues.Value)
        .Select(i =>
            new GithubIssue(
                $"Issue {i}",
                $"This is the body of issue {i}",
                $"Author {random.Next(1, 10)}"
            )
        );
    }
}