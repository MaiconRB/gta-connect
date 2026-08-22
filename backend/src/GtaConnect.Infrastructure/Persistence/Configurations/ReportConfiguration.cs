using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReporterProfileId).IsRequired();
        builder.Property(r => r.ReportedProfileId).IsRequired();
        builder.Property(r => r.Reason).IsRequired();

        builder.Property(r => r.Details).HasMaxLength(500);

        builder.Property(r => r.CreatedAtUtc).IsRequired();

        builder.Property(r => r.Status).IsRequired();

        // Sustenta tanto "todas as denúncias contra este perfil" quanto a agregação
        // por status usada no painel de revisão.
        builder.HasIndex(r => new { r.ReportedProfileId, r.Status });
    }
}
