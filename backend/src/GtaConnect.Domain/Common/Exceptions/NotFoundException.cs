namespace GtaConnect.Domain.Common.Exceptions;

public class NotFoundException : Exception
{
    /// <summary>Nome da entidade não encontrada (ex: "PlayerProfile"). Null quando a exceção foi criada com uma mensagem livre.</summary>
    public string? EntityName { get; }

    /// <summary>Chave usada na busca que falhou. Null quando a exceção foi criada com uma mensagem livre.</summary>
    public object? Key { get; }

    public NotFoundException(string message) : base(message)
    {
    }

    // A mensagem base da exceção aqui é só para log técnico (ex: ILogger) — o texto
    // exibido ao usuário é montado pela Api (GlobalExceptionHandler) via IStringLocalizer,
    // usando EntityName/Key. O Domain não pode depender de localização.
    public NotFoundException(string entityName, object key)
        : base($"{entityName}:{key}")
    {
        EntityName = entityName;
        Key = key;
    }
}
