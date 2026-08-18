namespace GtaConnect.Application.Features.Chat;

// SenderUserId/RecipientUserId são o ApplicationUserId (Identity) de cada lado — só o Hub
// usa esses dois campos, pra rotear via Clients.User(...); nunca vão pro MessageDto que o
// frontend recebe (o cliente não tem motivo pra conhecer ApplicationUserId, só ProfileId).
public record SendMessageResultDto(MessageDto Message, Guid SenderUserId, Guid RecipientUserId);
