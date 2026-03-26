using FluentValidation;
using FluentValidation.Results;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Application.Common.Execution;

internal static class ApplicationExecution
{
    public static async Task<Error?> ValidateAsync<TRequest>(
        TRequest request,
        IValidator<TRequest> validator,
        string errorCode,
        string defaultMessage,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(validator);

        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        return validationResult.IsValid
            ? null
            : BuildValidationError(validationResult, errorCode, defaultMessage);
    }

    public static async Task<Result<TResult>> ExecuteAsync<TResult>(
        Func<Task<Result<TResult>>> operation,
        string errorCode,
        Func<string, string, Error>? errorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Func<string, string, Error> resolvedErrorFactory = errorFactory ?? Error.Failure;

        try
        {
            return await operation();
        }
        catch (DomainException exception)
        {
            return Result.Failure<TResult>(resolvedErrorFactory(errorCode, exception.Message));
        }
    }

    public static async Task<Result> ExecuteAsync(
        Func<Task<Result>> operation,
        string errorCode,
        Func<string, string, Error>? errorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Func<string, string, Error> resolvedErrorFactory = errorFactory ?? Error.Failure;

        try
        {
            return await operation();
        }
        catch (DomainException exception)
        {
            return Result.Failure(resolvedErrorFactory(errorCode, exception.Message));
        }
    }

    public static Error BuildValidationError(
        ValidationResult validationResult,
        string errorCode,
        string defaultMessage)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        string message = string.Join(
            " | ",
            validationResult.Errors
                .Where(error => !string.IsNullOrWhiteSpace(error.ErrorMessage))
                .Select(error => error.ErrorMessage.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));

        return Error.Validation(
            errorCode,
            string.IsNullOrWhiteSpace(message)
                ? defaultMessage
                : message);
    }
}
