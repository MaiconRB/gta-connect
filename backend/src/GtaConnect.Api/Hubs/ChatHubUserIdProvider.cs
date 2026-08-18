using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace GtaConnect.Api.Hubs;

// Mesma lógica de ClaimsPrincipalExtensions.GetUserId() (Api/Common), pro Hub resolver o
// mesmo Id de usuário que os controllers resolvem via User.GetUserId(). O provider padrão
// do SignalR já leria ClaimTypes.NameIdentifier sozinho — mantido explícito aqui só pra
// documentar a dependência e garantir consistência caso o mapeamento de claims mude.
public class ChatHubUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? connection.User?.FindFirstValue("sub");
}
