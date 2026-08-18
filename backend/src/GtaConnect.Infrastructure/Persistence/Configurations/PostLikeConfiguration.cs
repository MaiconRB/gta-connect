using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.PostId).IsRequired();
        builder.Property(l => l.ProfileId).IsRequired();
        builder.Property(l => l.CreatedAtUtc).IsRequired();

        // Impede curtir o mesmo post duas vezes — toggle idempotente depende disso.
        builder.HasIndex(l => new { l.PostId, l.ProfileId }).IsUnique();
    }
}
