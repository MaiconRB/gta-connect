namespace GtaConnect.Application.Common.Interfaces;

/// <summary>
/// Abstrai onde/como a foto de avatar é armazenada. Implementação de hoje é disco local
/// (ver GtaConnect.Infrastructure.Storage.LocalDiskPhotoStorageService) — trocar por blob
/// storage em nuvem depois é uma migração isolada nessa camada, sem tocar Application/Domain.
/// </summary>
public interface IPhotoStorageService
{
    /// <summary>
    /// Salva o conteúdo da foto e devolve o caminho relativo servido (ex: "/uploads/avatars/{guid}.jpg").
    /// </summary>
    /// <param name="fileName">Nome original do arquivo — usado só para extrair a extensão, nunca reaproveitado como nome final.</param>
    Task<string> SaveAvatarAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>Mesma ideia de SaveAvatarAsync, mas salva em uploads/posts (ex: "/uploads/posts/{guid}.jpg").</summary>
    Task<string> SavePostPhotoAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
}
