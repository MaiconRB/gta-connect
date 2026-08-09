namespace GtaConnect.Application.Features.Profile;

public interface IProfileService
{
    Task<ProfileResponseDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ProfileResponseDto> UpdateMyProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default);

    Task<ProfileResponseDto> UploadAvatarAsync(Guid userId, Stream content, string fileName, long contentLength, CancellationToken cancellationToken = default);
}
