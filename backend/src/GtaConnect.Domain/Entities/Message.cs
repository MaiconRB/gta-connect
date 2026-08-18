namespace GtaConnect.Domain.Entities;

public class Message
{
    private const int MaxContentLength = 2000;

    public Guid Id { get; private set; }

    public Guid ConversationId { get; private set; }

    public Guid SenderProfileId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public DateTime SentAtUtc { get; private set; }

    private Message()
    {
    }

    public static Message Create(Guid conversationId, Guid senderProfileId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("A mensagem não pode ser vazia.", nameof(content));
        }

        var trimmedContent = content.Trim();
        if (trimmedContent.Length > MaxContentLength)
        {
            throw new ArgumentException($"A mensagem não pode ter mais de {MaxContentLength} caracteres.", nameof(content));
        }

        return new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderProfileId = senderProfileId,
            Content = trimmedContent,
            SentAtUtc = DateTime.UtcNow,
        };
    }
}
