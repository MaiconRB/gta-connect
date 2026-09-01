using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.DevSeed;

// Cria os usuários direto pelos blocos de construção de baixo nível (IIdentityService +
// PlayerProfile.Create), não via IAuthService.RegisterAsync — evita disparar e-mail de
// confirmação pra cada um dos ~24 perfis (vira ruído no Mailpit) e gerar JWT à toa, já
// que o seed não precisa de sessão nenhuma.
public class DevSeedService : IDevSeedService
{
    private const string SeedPassword = "Seed123!@#";
    private const string SeedEmailDomain = "gtaconnect.local";

    private static readonly string[] Names =
    [
        "Ragazzi_BR", "NightRider22", "HeistQueen", "LSStreetKing", "VicePulse",
        "GrindMasterSP", "FreewayFox", "MidnightCrew", "SandyShoresKid", "DelPerroDrift",
        "CayoRaider", "BunkerBoss77", "TequilaSunrise", "PalomaHeights", "RockfordHills_",
        "ChiliadClimber", "ParadiseGarage", "AltruistValley", "MirrorParkVibes", "DavisDrifter",
        "StripClubMVP", "CasinoRoyaleGTA", "MurrietaHeights", "PillboxHill",
    ];

    private static readonly string?[] Bios =
    [
        "Focado em heists, sempre com plano B.",
        "Só freemode e uma boa trilha sonora.",
        "Adoro corridas à noite, sem pressa de vencer.",
        "Explorando o mapa, sem hora pra voltar.",
        null,
        "Grinding pra próxima arma dourada.",
        "RP casual, sem drama.",
        "Investidor de bunker, sempre online cedo.",
        null,
        "Aqui pra jogar, não pra brigar.",
    ];

    private static readonly string?[] FavoriteModesOptions =
    [
        "Cayo Perico, Heists clássicos",
        "Corridas de rua",
        null,
        "Freemode, negócios",
        "Modo RP",
        null,
        "Deathmatch, PvP",
    ];

    private static readonly PlaystyleTag[] AllPlaystyleTags =
        Enum.GetValues<PlaystyleTag>().Where(t => t != PlaystyleTag.None && t != PlaystyleTag.All).ToArray();

    private static readonly AvailabilityTag[] AllAvailabilityTags =
        Enum.GetValues<AvailabilityTag>().Where(t => t != AvailabilityTag.None && t != AvailabilityTag.All).ToArray();

    private static readonly Region[] AllRegions = Enum.GetValues<Region>();

    private readonly IIdentityService _identityService;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly Random _random = new();

    public DevSeedService(IIdentityService identityService, IPlayerProfileRepository playerProfileRepository)
    {
        _identityService = identityService;
        _playerProfileRepository = playerProfileRepository;
    }

    public async Task<DevSeedResultDto> SeedAsync(CancellationToken cancellationToken = default)
    {
        var createdDisplayNames = new List<string>();

        foreach (var name in Names)
        {
            var email = $"seed.{name.ToLowerInvariant()}@{SeedEmailDomain}";

            var createResult = await _identityService.CreateUserAsync(email, SeedPassword);
            if (!createResult.Succeeded || createResult.UserId is not { } userId)
            {
                // E-mail já existe (RequireUniqueEmail do Identity) — é o próprio sinal de
                // idempotência: rodar o seed de novo só completa o que ainda falta, sem duplicar.
                continue;
            }

            var profile = PlayerProfile.Create(userId, name, RandomPlatform(), GameTitle.GtaV);
            await _playerProfileRepository.AddAsync(profile, cancellationToken);

            profile.UpdateProfile(
                bio: Bios[_random.Next(Bios.Length)],
                playstyleTags: RandomPlaystyleSubset(),
                hoursPlayed: _random.Next(0, 3000),
                favoriteModes: FavoriteModesOptions[_random.Next(FavoriteModesOptions.Length)],
                region: AllRegions[_random.Next(AllRegions.Length)],
                availabilityTags: RandomAvailabilitySubset());
            await _playerProfileRepository.UpdateAsync(profile, cancellationToken);

            createdDisplayNames.Add(name);
        }

        return new DevSeedResultDto(createdDisplayNames.Count, createdDisplayNames);
    }

    private Platform RandomPlatform() => _random.Next(2) == 0 ? Platform.Ps4 : Platform.Ps5;

    // Sorteia um subconjunto realista (não todas as 8/5 tags de uma vez — perfil real de
    // verdade marca 2-4 tags, não o conjunto inteiro) usando Fisher-Yates.
    private PlaystyleTag RandomPlaystyleSubset()
    {
        var shuffled = Shuffle(AllPlaystyleTags);
        var count = _random.Next(1, 5);
        return shuffled.Take(count).Aggregate(PlaystyleTag.None, (mask, tag) => mask | tag);
    }

    private AvailabilityTag RandomAvailabilitySubset()
    {
        var shuffled = Shuffle(AllAvailabilityTags);
        var count = _random.Next(1, 4);
        return shuffled.Take(count).Aggregate(AvailabilityTag.None, (mask, tag) => mask | tag);
    }

    private T[] Shuffle<T>(T[] source)
    {
        var copy = (T[])source.Clone();
        for (var i = copy.Length - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }

        return copy;
    }
}
