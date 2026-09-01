using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Application.Resources;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Application.Features.Reputation;

public class GameSessionService : IGameSessionService
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly INotificationService _notificationService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GameSessionService(
        IGameSessionRepository gameSessionRepository,
        IConnectionRepository connectionRepository,
        IPlayerProfileRepository playerProfileRepository,
        INotificationService notificationService,
        IStringLocalizer<SharedResource> localizer)
    {
        _gameSessionRepository = gameSessionRepository;
        _connectionRepository = connectionRepository;
        _playerProfileRepository = playerProfileRepository;
        _notificationService = notificationService;
        _localizer = localizer;
    }

    public async Task<Guid> LogSessionAsync(Guid userId, Guid connectionId, CancellationToken cancellationToken = default)
    {
        var myProfile = await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);

        var connection = await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);

        // Só registra sessão numa conexão ACEITA da qual eu participo — mesma regra que
        // já valia pra avaliação antes da sessão existir.
        if (connection is null || !connection.HasParticipant(myProfile.Id) || connection.Status != ConnectionStatus.Accepted)
        {
            throw new ValidationAppException("connectionId", _localizer["GameSession_ConnectionRequired"]);
        }

        var session = GameSession.Create(connectionId, myProfile.Id);
        await _gameSessionRepository.AddAsync(session, cancellationToken);

        var otherProfileId = connection.GetOtherParticipantId(myProfile.Id);
        await _notificationService.NotifyAsync(otherProfileId, myProfile.Id, NotificationType.SessionLogged, session.Id, cancellationToken);

        return session.Id;
    }
}
