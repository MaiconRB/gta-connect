namespace GtaConnect.Application.Common;

/// <summary>
/// Opcoes da camada Application que dependem de valores externos (ex: URL do frontend).
/// Registradas em Program.cs via services.Configure<AuthOptions> e injetadas via IOptions<AuthOptions>.
/// </summary>
public class AuthOptions
{
    /// <summary>URL base do frontend — usada para montar links de e-mail (confirmar e-mail, reset de senha etc.).</summary>
    public string FrontendUrl { get; set; } = "http://localhost:4200";
}
