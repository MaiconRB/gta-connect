using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Reputation;

// Tudo nullable de propósito — representa "nunca houve pedido entre nós dois" quando vazio.
public record ConnectionStatusDto(Guid? ConnectionId, ConnectionStatus? Status, bool? IsRequester, int? MyRatingScore);
