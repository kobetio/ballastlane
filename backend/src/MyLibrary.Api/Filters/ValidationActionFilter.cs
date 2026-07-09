using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyLibrary.Api.Filters;

/// <summary>
/// Global action filter that runs the FluentValidation validator (if any is registered)
/// for every action argument before the action executes. On failure it throws
/// <see cref="ValidationException"/>, which <c>ExceptionHandlingMiddleware</c> turns into
/// a consistent HTTP 400 response. Keeps controllers free of manual validation calls.
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        await next();
    }
}
