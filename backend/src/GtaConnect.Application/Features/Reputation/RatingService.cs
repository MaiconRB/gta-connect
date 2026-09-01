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
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly INotificationService _notificationService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RatingService(
        IRatingRepository ratingRepository,
        IGameSessionRepository gameSessionRepository,
        IConnectionRepository connectionRepository,
        IPlayerProfileRepository playerProfileRepository,
        IBlockRepository blockRepository,
        INotificationService notificationService,
        IStringLocalizer<SharedResource> localizer)
    {
        _ratingRepository = ratingRepository;
        _gameSessionRepository = gameSessionRepository;
        _connectionRepository = connectionRepository;
        _playerProfileRepository = playerProfileRepository;
        _blockRepository = blockRepository;
        _notificationService = notificationService;
        _localizer = localizer;
    }

    public async Task RateAsync(
        Guid userId,
        Guid gameSessionId,
        int score,
        string? comment,
        bool completedSession,
        bool knewWhatToDo,
        bool wasToxic,
        CancellationToken cancellationToken = default)
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

        var gameSession = await _gameSessionRepository.GetByIdAsync(gameSessionId, cancellationToken)
            ?? throw new ValidationAppException("gameSessionId", _localizer["Rating_SessionRequired"]);

        var connection = await _connectionRepository.GetByIdAsync(gameSession.ConnectionId, cancellationToken)
            ?? throw new ValidationAppException("gameSessionId", _localizer["Rating_SessionRequired"]);

        // Só quem participou da sessão (ou seja, é parte da Connection dela) pode avaliar.
        if (!connection.HasParticipant(myProfile.Id))
        {
            throw new ValidationAppException("gameSessionId", _localizer["Rating_SessionRequired"]);
        }

        var ratedProfileId = connection.GetOtherParticipantId(myProfile.Id);

        // Defesa em profundidade — na prática, dificilmente dá pra ficar bloqueado com
        // quem se tem uma conexão aceita, mas a checagem mantém a mesma consistência
        // já aplicada em busca/chat/feed.
        if (await _blockRepository.ExistsEitherDirectionAsync(myProfile.Id, ratedProfileId, cancellationToken))
        {
            throw new ValidationAppException("profileId", _localizer["Connection_Blocked"]);
        }

        var existingRating = await _ratingRepository.FindBySessionAsync(gameSessionId, myProfile.Id, ratedProfileId, cancellationToken);
        if (existingRating is null)
        {
            var rating = Rating.Create(gameSessionId, myProfile.Id, ratedProfileId, score, comment, completedSession, knewWhatToDo, wasToxic);
            await _ratingRepository.AddAsync(rating, cancellationToken);
        }
        else
        {
            existingRating.Update(score, comment, completedSession, knewWhatToDo, wasToxic);
            await _ratingRepository.UpdateAsync(existingRating, cancellationToken);
        }

        await _notificationService.NotifyAsync(ratedProfileId, myProfile.Id, NotificationType.RatingReceived, null, cancellationToken);
    }
}
