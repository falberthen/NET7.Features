namespace NET7.Features.MinimalAPI.Tests;

public class TypedResultsTests
{
    [Fact]
    public async Task HandleList_WhenHandlingRequest_ShouldReturnOKResponseWith10Issues()
    {
        // Arrange
        var take = 10;
        var handler = new GithubIssuesRouteHandler();

        // Act
        var result = await handler.HandleList(take);

        // Assert
        Assert.IsType<Ok<List<GithubIssue>>>(result);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.Count == take);
    }
}