using GtaConnect.Api.Common;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Chat;
using GtaConnect.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GtaConnect.Api.Hubs;

// SignalR não passa pelo GlobalExceptionHandler HTTP nem pelo RequestLocalizationMiddleware
// por invocação — por isso as mensagens de erro aqui são fixas em pt-BR (mesmo nível de
// simplicidade das exceções de domínio, que também não são localizadas via .resx).
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IPresenceTracker _presenceTracker;

    public ChatHub(IChatService chatService, IPresenceTracker presenceTracker)
    {
        _chatService = chatService;
        _presenceTracker = presenceTracker;
    }

    // "Online agora" é só reflexo de ter essa conexão SignalR aberta — a mesma que já existe
    // pro chat/notificações. Sem hub/infra novos.
    public override Task OnConnectedAsync()
    {
        _presenceTracker.MarkOnline(Context.User!.GetUserId());
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _presenceTracker.MarkOffline(Context.User!.GetUserId());
        return base.OnDisconnectedAsync(exception);
    }

    // Retorna a mensagem persistida pra quem chamou — o cliente usa isso pra descobrir o
    // Id da conversa na primeira mensagem (antes disso, só conhece o Id do outro perfil).
    // O broadcast abaixo é quem mantém outras abas/dispositivos (inclusive o próprio
    // remetente) sincronizados depois disso.
    public async Task<MessageDto> SendMessage(Guid recipientProfileId, string content)
    {
        var senderUserId = Context.User!.GetUserId();

        SendMessageResultDto result;
        try
        {
            result = await _chatService.SendMessageAsync(senderUserId, recipientProfileId, content, Context.ConnectionAborted);
        }
        catch (ArgumentException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (NotFoundException)
        {
            throw new HubException("Jogador não encontrado.");
        }

        await Clients.User(result.SenderUserId.ToString()).SendAsync("ReceiveMessage", result.Message);
        await Clients.User(result.RecipientUserId.ToString()).SendAsync("ReceiveMessage", result.Message);

        return result.Message;
    }

    public async Task MarkAsRead(Guid conversationId)
    {
        var userId = Context.User!.GetUserId();

        try
        {
            await _chatService.MarkAsReadAsync(userId, conversationId, Context.ConnectionAborted);
        }
        catch (NotFoundException)
        {
            throw new HubException("Conversa não encontrada.");
        }
    }
}
