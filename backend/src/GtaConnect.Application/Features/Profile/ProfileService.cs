using FluentValidation;
using GtaConnect.Application.Common.Extensions;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Resources;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Application.Features.Profile;

public class ProfileService : IProfileService
{
    private static readonly string[] AllowedAvatarExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxAvatarSizeBytes = 2 * 1024 * 1024;

    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IPhotoStorageService _photoStorageService;
    private readonly IValidator<UpdateProfileRequestDto> _updateProfileValidator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ProfileService(
        IPlayerProfileRepository playerProfileRepository,
        IPhotoStorageService photoStorageService,
        IValidator<UpdateProfileRequestDto> updateProfileValidator,
        IStringLocalizer<SharedResource> localizer)
    {
        _playerProfileRepository = playerProfileRepository;
        _photoStorageService = photoStorageService;
        _updateProfileValidator = updateProfileValidator;
        _localizer = localizer;
    }

    public async Task<ProfileResponseDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);
        return ToDto(profile);
    }

    public async Task<ProfileResponseDto> UpdateMyProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        await _updateProfileValidator.ValidateAndThrowAppExceptionAsync(request, cancellationToken);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);
        profile.UpdateProfile(request.Bio, request.PlaystyleTags, request.HoursPlayed, request.FavoriteModes, request.Region, request.AvailabilityTags);
        await _playerProfileRepository.UpdateAsync(profile, cancellationToken);

        return ToDto(profile);
    }

    public async Task<ProfileResponseDto> UploadAvatarAsync(Guid userId, Stream content, string fileName, long contentLength, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedAvatarExtensions.Contains(extension))
        {
            throw new ValidationAppException("avatar", _localizer["Profile_AvatarInvalidType"]);
        }

        if (contentLength > MaxAvatarSizeBytes)
        {
            throw new ValidationAppException("avatar", _localizer["Profile_AvatarTooLarge"]);
        }

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var avatarPath = await _photoStorageService.SaveAvatarAsync(content, fileName, cancellationToken);
        profile.SetAvatar(avatarPath);
        await _playerProfileRepository.UpdateAsync(profile, cancellationToken);

        return ToDto(profile);
    }

    private async Task<PlayerProfile> GetProfileOrThrowAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);
    }

    private static ProfileResponseDto ToDto(PlayerProfile profile) => new(
        profile.Id,
        profile.DisplayName,
        profile.Platform,
        profile.GameTitle,
        profile.Bio,
        profile.PlaystyleTags,
        profile.HoursPlayed,
        profile.FavoriteModes,
        profile.Region,
        profile.AvailabilityTags,
        profile.AvatarPath,
        profile.CreatedAtUtc);
}
