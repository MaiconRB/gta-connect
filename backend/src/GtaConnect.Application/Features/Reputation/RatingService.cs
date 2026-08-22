using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Application.Resources;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Application.Features.Reputation;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly INotificationService _notificationService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RatingService(
        IRatingRepository ratingRepository,
        IConnectionRepository connectionRepository,
        IPlayerProfileRepository playerProfileRepository,
        IBlockRepository blockRepository,
        INotificationService notificationService,
        IStringLocalizer<SharedResource> localizer)
    {
        _ratingRepository = ratingRepository;
        _connectionRepository = connectionRepository;
        _playerProfileRepository = playerProfileRepository;
        _blockRepository = blockRepository;
        _notificationService = notificationService;
        _localizer = localizer;
    }

    public async Task RateAsync(Guid userId, Guid targetProfileId, int score, string? comment, CancellationToken cancellationToken = default)
    {
        // Checagem AQUI, antes do domínio — ArgumentOutOfRangeException não é mapeada pelo
        // GlobalExceptionHandler (viraria 500), e nota fora do intervalo é um erro de
        // usuário genuinamente alcançável (mesma lição do post vazio no Feed).
        if (score is < 1 or > 5)
        {
            throw new ValidationAppException("score", _localizer["Rating_ScoreOutOfRange"]);
        }

        var myProfile = await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);

        var targetProfile = await _playerProfileRepository.GetByIdAsync(targetProfileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), targetProfileId);

        var connection = await _connectionRepository.FindBetweenAsync(myProfile.Id, targetProfile.Id, cancellationToken);
        if (connection is null || connection.Status != ConnectionStatus.Accepted)
        {
            throw new ValidationAppException("profileId", _localizer["Rating_ConnectionRequired"]);
        }

        // Defesa em profundidade — na prática, dificilmente dá pra ficar bloqueado com
        // quem se tem uma conexão aceita, mas a checagem mantém a mesma consistência
        // já aplicada em busca/chat/feed.
        if (await _blockRepository.ExistsEitherDirectionAsync(myProfile.Id, targetProfile.Id, cancellationToken))
        {
            throw new ValidationAppException("profileId", _localizer["Connection_Blocked"]);
        }

        var existingRating = await _ratingRepository.FindAsync(myProfile.Id, targetProfile.Id, cancellationToken);
        if (existingRating is null)
        {
            var rating = Rating.Create(myProfile.Id, targetProfile.Id, score, comment);
            await _ratingRepository.AddAsync(rating, cancellationToken);
        }
        else
        {
            existingRating.Update(score, comment);
            await _ratingRepository.UpdateAsync(existingRating, cancellationToken);
        }

        await _notificationService.NotifyAsync(targetProfile.Id, myProfile.Id, NotificationType.RatingReceived, null, cancellationToken);
    }
}
