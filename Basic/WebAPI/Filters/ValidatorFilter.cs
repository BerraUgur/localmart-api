using FluentValidation;
using WebAPI.Extensions;

namespace WebAPI.Filters;

public class ValidatorFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context?.Arguments == null)
            return Results.BadRequest();

        var validatable = context.Arguments.SingleOrDefault(x => x?.GetType() == typeof(T));
        if (validatable is not T validatableObj)
            return Results.BadRequest();

        if (validator == null)
            return Results.BadRequest("Validator is not available.");

        var validationResult = await validator.ValidateAsync(validatableObj);

        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors.ToRespose());

        var result = await next(context);
        return result;
    }
}