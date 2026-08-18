using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ConversationId).IsRequired();
        builder.Property(m => m.SenderProfileId).IsRequired();

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.SentAtUtc).IsRequired();

        // Sustenta tanto a paginação do histórico (ORDER BY SentAtUtc) quanto a contagem
        // de não-lidas (COUNT correlacionado por ConversationId + SentAtUtc).
        builder.HasIndex(m => new { m.ConversationId, m.SentAtUtc });
    }
}
