using GtaConnect.Api.Common;
using GtaConnect.Application.Features.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileResponseDto>> GetMe(CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetMyProfileAsync(User.GetUserId(), cancellationToken);
        return Ok(WithAbsoluteAvatarUrl(profile));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ProfileResponseDto>> UpdateMe(UpdateProfileRequestDto request, CancellationToken cancellationToken)
    {
        var profile = await _profileService.UpdateMyProfileAsync(User.GetUserId(), request, cancellationToken);
        return Ok(WithAbsoluteAvatarUrl(profile));
    }

    // Guarda-corpo real de tamanho — sem isso, o Kestrel/FormOptions aceitariam um upload
    // bem maior antes de qualquer validação de código (tipo/tamanho) rodar na Application.
    [HttpPost("me/avatar")]
    [RequestSizeLimit(2 * 1024 * 1024 + 1024)]
    public async Task<ActionResult<ProfileResponseDto>> UploadAvatar(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest();
        }

        await using var stream = file.OpenReadStream();
        var profile = await _profileService.UploadAvatarAsync(User.GetUserId(), stream, file.FileName, file.Length, cancellationToken);
        return Ok(WithAbsoluteAvatarUrl(profile));
    }

    // AvatarPath vem da Application como caminho relativo ("/uploads/avatars/xxx.jpg").
    // A Api é a única camada que conhece Request.Scheme/Host, então é aqui — e só aqui —
    // que o caminho relativo vira URL absoluta pronta pro frontend usar num <img src>.
    private ProfileResponseDto WithAbsoluteAvatarUrl(ProfileResponseDto profile)
    {
        if (profile.AvatarPath is null)
        {
            return profile;
        }

        var absoluteUrl = $"{Request.Scheme}://{Request.Host}{profile.AvatarPath}";
        return profile with { AvatarPath = absoluteUrl };
    }
}
