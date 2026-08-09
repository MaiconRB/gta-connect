using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Platform)
            .IsRequired();

        builder.Property(p => p.GameTitle)
            .IsRequired();

        builder.Property(p => p.Bio)
            .HasMaxLength(500);

        builder.Property(p => p.PlaystyleTags)
            .IsRequired();

        builder.Property(p => p.HoursPlayed)
            .IsRequired();

        builder.Property(p => p.FavoriteModes)
            .HasMaxLength(200);

        builder.Property(p => p.AvatarPath)
            .HasMaxLength(300);

        // Um ApplicationUser tem, por enquanto, um único PlayerProfile (relação 1:1 lógica).
        // Quando o app passar a suportar múltiplos jogos (GTA V + GTA VI), isso pode evoluir
        // para permitir mais de um perfil por usuário — por ora, o índice único mantém a regra.
        builder.HasIndex(p => p.ApplicationUserId).IsUnique();
    }
}
