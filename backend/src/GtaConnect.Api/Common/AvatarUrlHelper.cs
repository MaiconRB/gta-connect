using GtaConnect.Application.Features.PlayerSearch;
using GtaConnect.Application.Features.Profile;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Common;

// AvatarPath sempre chega da Application como caminho relativo ("/uploads/avatars/xxx.jpg")
// ou null — a Api é a única camada que conhece Request.Scheme/Host, então é aqui — e só
// aqui — que o caminho relativo vira URL absoluta pronta pro frontend usar num <img src>.
public static class AvatarUrlHelper
{
    public static string ToAbsoluteUrl(this HttpRequest request, string relativePath) =>
        $"{request.Scheme}://{request.Host}{relativePath}";

    public static ProfileResponseDto WithAbsoluteAvatarUrl(this ControllerBase controller, ProfileResponseDto profile) =>
        profile.AvatarPath is null ? profile : profile with { AvatarPath = controller.Request.ToAbsoluteUrl(profile.AvatarPath) };

    public static PlayerSummaryDto WithAbsoluteAvatarUrl(this ControllerBase controller, PlayerSummaryDto summary) =>
        summary.AvatarPath is null ? summary : summary with { AvatarPath = controller.Request.ToAbsoluteUrl(summary.AvatarPath) };
}
