using FluentValidation;
using GtaConnect.Domain.Common.Exceptions;

namespace GtaConnect.Application.Common.Extensions;

public static class ValidatorExtensions
{
    /// <summary>
    /// Valida e, em caso de falha, lança ValidationAppException (nossa exceção de domínio/aplicação)
    /// em vez da FluentValidation.ValidationException nativa — mantém o resto da aplicação
    /// desacoplado da biblioteca de validação escolhida.
    /// </summary>
    public static async Task ValidateAndThrowAppExceptionAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new ValidationAppException(errors);
        }
    }
}
