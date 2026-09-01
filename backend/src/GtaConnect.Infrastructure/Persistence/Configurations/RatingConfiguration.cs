using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.GameSessionId).IsRequired();
        builder.Property(r => r.RaterProfileId).IsRequired();
        builder.Property(r => r.RatedProfileId).IsRequired();
        builder.Property(r => r.Score).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(500);
        builder.Property(r => r.CompletedSession).IsRequired();
        builder.Property(r => r.KnewWhatToDo).IsRequired();
        builder.Property(r => r.WasToxic).IsRequired();
        builder.Property(r => r.CreatedAtUtc).IsRequired();

        // Um avaliador só tem UMA avaliação por avaliado POR SESSÃO — avaliar a mesma sessão
        // de novo faz upsert (ver RatingService), não cria um segundo registro. Isso é o que
        // muda de "impressão geral sobrescrita" pra "histórico por sessão jogada".
        builder.HasIndex(r => new { r.GameSessionId, r.RaterProfileId, r.RatedProfileId }).IsUnique();

        // Sustenta a agregação em lote (GetAggregatesAsync) por RatedProfileId.
        builder.HasIndex(r => r.RatedProfileId);
    }
}
