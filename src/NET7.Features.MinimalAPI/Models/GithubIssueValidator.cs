namespace NET7.Features.MinimalAPI.Models;

public class GithubIssueValidator : AbstractValidator<GithubIssue>
{
    public GithubIssueValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.Author).NotEmpty();
    }
}