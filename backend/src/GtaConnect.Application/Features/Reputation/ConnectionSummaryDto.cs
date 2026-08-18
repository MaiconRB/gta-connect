using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Reputation;

// IsRequester diz se EU mandei o pedido (true) ou recebi (false) — o frontend usa isso
// pra decidir em qual das três seções da tela de conexões o item entra.
public record ConnectionSummaryDto(
    Guid ConnectionId,
    Guid OtherProfileId,
    string OtherDisplayName,
    string? OtherAvatarPath,
    ConnectionStatus Status,
    bool IsRequester,
    DateTime CreatedAtUtc);
