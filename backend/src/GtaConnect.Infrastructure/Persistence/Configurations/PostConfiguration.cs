using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.AuthorProfileId).IsRequired();
        builder.Property(p => p.Content).HasMaxLength(1000);
        builder.Property(p => p.PhotoPath).HasMaxLength(300);
        builder.Property(p => p.CreatedAtUtc).IsRequired();

        // Sustenta a ordenação do feed (mais recente primeiro).
        builder.HasIndex(p => p.CreatedAtUtc);
    }
}
