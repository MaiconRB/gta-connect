namespace GtaConnect.Application.Features.DevSeed;

public interface IDevSeedService
{
    /// <summary>
    /// Popula o banco com perfis de teste variados (tags/região/disponibilidade sorteadas).
    /// Idempotente: perfis já existentes (mesmo e-mail) são pulados, não duplicados.
    /// </summary>
    Task<DevSeedResultDto> SeedAsync(CancellationToken cancellationToken = default);
}
