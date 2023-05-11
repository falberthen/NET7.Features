namespace NET7.Features.MinimalAPI.Models;

public class GithubIssue
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; private set; }
    public string Body { get; private set; }
    public string Author { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    public GithubIssue(
        string title, 
        string body, 
        string author)
    {
        Title = title;
        Body = body;
        Author = author;
    }    
}