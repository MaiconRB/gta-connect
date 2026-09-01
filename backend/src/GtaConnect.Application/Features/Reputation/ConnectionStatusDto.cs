using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Reputation;

// Tudo nullable de propósito — representa "nunca houve pedido entre nós dois" quando vazio.
// MyRatingScore agora se refere à sessão mais recente (LatestSessionId) — Rating é por sessão,
// não mais um upsert único por par, então "minha nota" só faz sentido junto da sessão dela.
public record ConnectionStatusDto(Guid? ConnectionId, ConnectionStatus? Status, bool? IsRequester, Guid? LatestSessionId, int? MyRatingScore);
