using System.Collections.ObjectModel;

namespace GtaConnect.Domain.Common.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de validação de entrada ou de negócio falha.
/// Nome "ValidationAppException" (não "ValidationException") para não colidir com
/// System.ComponentModel.DataAnnotations.ValidationException.
/// </summary>
public class ValidationAppException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("Um ou mais erros de validação ocorreram.")
    {
        Errors = new ReadOnlyDictionary<string, string[]>(errors);
    }

    public ValidationAppException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] })
    {
    }
}
