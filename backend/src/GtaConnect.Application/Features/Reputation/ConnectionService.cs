using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Application.Resources;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Application.Features.Reputation;

public class ConnectionService : IConnectionService
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly INotificationService _notificationService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ConnectionService(
        IConnectionRepository connectionRepository,
        IRatingRepository ratingRepository,
        IGameSessionRepository gameSessionRepository,
        IPlayerProfileRepository playerProfileRepository,
        IBlockRepository blockRepository,
        INotificationService notificationService,
        IStringLocalizer<SharedResource> localizer)
    {
        _connectionRepository = connectionRepository;
        _ratingRepository = ratingRepository;
        _gameSessionRepository = gameSessionRepository;
        _playerProfileRepository = playerProfileRepository;
        _blockRepository = blockRepository;
        _notificationService = notificationService;
        _localizer = localizer;
    }

    public async Task<ConnectionSummaryDto> SendRequestAsync(Guid userId, Guid targetProfileId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        var targetProfile = await _playerProfileRepository.GetByIdAsync(targetProfileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), targetProfileId);

        // Consistência com busca/chat/feed: bloqueados não conseguem se conectar.
        if (await _blockRepository.ExistsEitherDirectionAsync(myProfile.Id, targetProfile.Id, cancellationToken))
        {
            throw new ValidationAppException("profileId", _localizer["Connection_Blocked"]);
        }

        var existing = await _connectionRepository.FindBetweenAsync(myProfile.Id, targetProfile.Id, cancellationToken);

        // Um Declined antigo não bloqueia um pedido novo — só Pending/Accepted contam
        // como "já existe conexão entre os dois".
        if (existing is not null && existing.Status != ConnectionStatus.Declined)
        {
            return ToSummaryDto(existing, myProfile.Id, targetProfile);
        }

        var connection = Connection.Create(myProfile.Id, targetProfile.Id);
        await _connectionRepository.AddAsync(connection, cancellationToken);

        await _notificationService.NotifyAsync(targetProfile.Id, myProfile.Id, NotificationType.ConnectionRequestReceived, connection.Id, cancellationToken);

        return ToSummaryDto(connection, myProfile.Id, targetProfile);
    }

    public async Task AcceptAsync(Guid userId, Guid connectionId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        var connection = await GetConnectionForParticipantOrThrowAsync(connectionId, myProfile.Id, cancellationToken);

        connection.Accept(myProfile.Id);
        await _connectionRepository.UpdateStatusAsync(connection, cancellationToken);
    }

    public async Task DeclineAsync(Guid userId, Guid connectionId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        var connection = await GetConnectionForParticipantOrThrowAsync(connectionId, myProfile.Id, cancellationToken);

        connection.Decline(myProfile.Id);
        await _connectionRepository.UpdateStatusAsync(connection, cancellationToken);
    }

    public async Task<IReadOnlyList<ConnectionSummaryDto>> GetConnectionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        return await _connectionRepository.GetConnectionsForProfileAsync(myProfile.Id, cancellationToken);
    }

    public async Task<ConnectionStatusDto> GetConnectionStatusAsync(Guid userId, Guid targetProfileId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        var connection = await _connectionRepository.FindBetweenAsync(myProfile.Id, targetProfileId, cancellationToken);

        if (connection is null)
        {
            return new ConnectionStatusDto(null, null, null, null, null);
        }

        // "Minha nota" agora é sempre relativa à sessão mais recente da conexão — Rating é
        // por sessão, então não existe mais "uma nota geral" sem uma sessão associada.
        var latestSession = await _gameSessionRepository.GetLatestForConnectionAsync(connection.Id, cancellationToken);
        int? myRatingScore = null;
        if (latestSession is not null)
        {
            var myRating = await _ratingRepository.FindBySessionAsync(latestSession.Id, myProfile.Id, targetProfileId, cancellationToken);
            myRatingScore = myRating?.Score;
        }

        return new ConnectionStatusDto(connection.Id, connection.Status, connection.RequesterProfileId == myProfile.Id, latestSession?.Id, myRatingScore);
    }

    private async Task<PlayerProfile> GetProfileOrThrowAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);
    }

    // "Não existe" cobre tanto conexão inexistente quanto conexão alheia — mesmo padrão
    // já usado em conversas.
    private async Task<Connection> GetConnectionForParticipantOrThrowAsync(Guid connectionId, Guid profileId, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null || !connection.HasParticipant(profileId))
        {
            throw new NotFoundException(nameof(Connection), connectionId);
        }

        return connection;
    }

    private static ConnectionSummaryDto ToSummaryDto(Connection connection, Guid viewerProfileId, PlayerProfile otherProfile) => new(
        connection.Id,
        otherProfile.Id,
        otherProfile.DisplayName,
        otherProfile.AvatarPath,
        connection.Status,
        connection.RequesterProfileId == viewerProfileId,
        connection.CreatedAtUtc);
}
