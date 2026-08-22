using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.RecipientProfileId).IsRequired();
        builder.Property(n => n.ActorProfileId).IsRequired();
        builder.Property(n => n.Type).IsRequired();
        builder.Property(n => n.IsRead).IsRequired();
        builder.Property(n => n.CreatedAtUtc).IsRequired();

        // Sustenta tanto a contagem de não-lidas quanto a listagem recente do destinatário.
        builder.HasIndex(n => new { n.RecipientProfileId, n.IsRead });
    }
}
