using GtaConnect.Api.Common;
using GtaConnect.Application.Common;
using GtaConnect.Application.Features.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ConversationsController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationSummaryDto>>> GetConversations(CancellationToken cancellationToken)
    {
        var conversations = await _chatService.GetConversationsAsync(User.GetUserId(), cancellationToken);
        return Ok(conversations.Select(this.WithAbsoluteAvatarUrl).ToList());
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<PagedResultDto<MessageDto>>> GetMessages(Guid id, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var messages = await _chatService.GetMessagesAsync(User.GetUserId(), id, page, pageSize, cancellationToken);
        return Ok(messages);
    }

    [HttpGet("with/{profileId:guid}")]
    public async Task<ActionResult<ConversationSummaryDto>> GetConversationWith(Guid profileId, CancellationToken cancellationToken)
    {
        var conversation = await _chatService.GetConversationWithAsync(User.GetUserId(), profileId, cancellationToken);
        return conversation is null ? NotFound() : Ok(this.WithAbsoluteAvatarUrl(conversation));
    }
}
