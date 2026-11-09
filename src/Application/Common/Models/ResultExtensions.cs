using BaseApp.Application.Common.Exceptions;
using FluentValidation.Results;

namespace BaseApp.Application.Common.Models;

public static class ResultExtensions
{
    public static void ThrowIfFailed(this Result result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new Exceptions.ValidationException(result.Errors.Select(e => new ValidationFailure("Error", e)));
    }
}
