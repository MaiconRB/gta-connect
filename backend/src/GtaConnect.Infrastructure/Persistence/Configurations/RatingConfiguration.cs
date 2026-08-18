using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RaterProfileId).IsRequired();
        builder.Property(r => r.RatedProfileId).IsRequired();
        builder.Property(r => r.Score).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(500);
        builder.Property(r => r.CreatedAtUtc).IsRequired();

        // Um avaliador só tem UMA avaliação por avaliado — avaliar de novo faz upsert
        // (ver RatingService), não cria um segundo registro.
        builder.HasIndex(r => new { r.RaterProfileId, r.RatedProfileId }).IsUnique();

        // Sustenta a agregação em lote (GetAggregatesAsync) por RatedProfileId.
        builder.HasIndex(r => r.RatedProfileId);
    }
}
