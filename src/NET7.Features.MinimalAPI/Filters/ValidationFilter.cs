namespace NET7.Features.MinimalAPI.Filters;

public class ValidationFilter<T> : IEndpointFilter 
    where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var objectArgument = context.GetArgument<T>(0);
        var result = await _validator.ValidateAsync(objectArgument!);

        if (!result.IsValid)
            return TypedResults.BadRequest(result.Errors);

        return await next(context);
    }
}