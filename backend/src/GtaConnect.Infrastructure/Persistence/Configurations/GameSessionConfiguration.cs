using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ConnectionId).IsRequired();
        builder.Property(s => s.LoggedByProfileId).IsRequired();
        builder.Property(s => s.PlayedAtUtc).IsRequired();
        builder.Property(s => s.CreatedAtUtc).IsRequired();

        // Sustenta GetLatestForConnectionAsync (ORDER BY PlayedAtUtc DESC filtrado por ConnectionId).
        builder.HasIndex(s => new { s.ConnectionId, s.PlayedAtUtc });
    }
}
