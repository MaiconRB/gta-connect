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

    public async Task<string> SaveAvatarAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var webRootPath = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var avatarsDirectory = Path.Combine(webRootPath, "uploads", "avatars");
        Directory.CreateDirectory(avatarsDirectory);

        // Nunca reaproveita o nome vindo do cliente além da extensão — evita colisão,
        // overwrite acidental e path traversal.
        var extension = Path.GetExtension(fileName);
        var generatedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(avatarsDirectory, generatedFileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return $"/uploads/avatars/{generatedFileName}";
    }
}
