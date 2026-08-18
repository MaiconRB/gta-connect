using GtaConnect.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace GtaConnect.Infrastructure.Storage;

/// <summary>
/// Salva fotos de avatar em disco, dentro de wwwroot/uploads/avatars — servido depois
/// via UseStaticFiles() na Api. wwwroot pode não existir ainda num clone limpo do repo,
/// então a pasta é criada aqui mesmo, na primeira gravação.
/// </summary>
public class LocalDiskPhotoStorageService : IPhotoStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalDiskPhotoStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public Task<string> SaveAvatarAsync(Stream content, string fileName, CancellationToken cancellationToken = default) =>
        SavePhotoAsync(content, fileName, "avatars", cancellationToken);

    public Task<string> SavePostPhotoAsync(Stream content, string fileName, CancellationToken cancellationToken = default) =>
        SavePhotoAsync(content, fileName, "posts", cancellationToken);

    private async Task<string> SavePhotoAsync(Stream content, string fileName, string subfolder, CancellationToken cancellationToken)
    {
        var webRootPath = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var directory = Path.Combine(webRootPath, "uploads", subfolder);
        Directory.CreateDirectory(directory);

        // Nunca reaproveita o nome vindo do cliente além da extensão — evita colisão,
        // overwrite acidental e path traversal.
        var extension = Path.GetExtension(fileName);
        var generatedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(directory, generatedFileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return $"/uploads/{subfolder}/{generatedFileName}";
    }
}
