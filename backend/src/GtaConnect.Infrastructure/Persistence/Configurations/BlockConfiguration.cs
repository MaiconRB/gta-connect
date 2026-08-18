using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BlockerProfileId).IsRequired();
        builder.Property(b => b.BlockedProfileId).IsRequired();
        builder.Property(b => b.CreatedAtUtc).IsRequired();

        // Impede duplicar o mesmo bloqueio na mesma direção — não impede A→B e B→A
        // coexistirem (dois registros distintos), o que é aceitável.
        builder.HasIndex(b => new { b.BlockerProfileId, b.BlockedProfileId }).IsUnique();
    }
}
