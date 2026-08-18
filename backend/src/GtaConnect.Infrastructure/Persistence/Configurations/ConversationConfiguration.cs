using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ParticipantAId).IsRequired();
        builder.Property(c => c.ParticipantBId).IsRequired();
        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.LastMessageAtUtc).IsRequired();

        // Garante, em nível de banco, que só existe uma conversa por par de participantes —
        // ParticipantAId é sempre o menor Guid (normalizado em Conversation.Create), então
        // o par é único independente de quem inicia a conversa.
        builder.HasIndex(c => new { c.ParticipantAId, c.ParticipantBId }).IsUnique();
    }
}
