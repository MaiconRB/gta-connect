using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtaConnect.Infrastructure.Persistence.Configurations;

public class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.RequesterProfileId).IsRequired();
        builder.Property(c => c.AddresseeProfileId).IsRequired();
        builder.Property(c => c.Status).IsRequired();
        builder.Property(c => c.CreatedAtUtc).IsRequired();

        // Rede de segurança contra corrida — a checagem "já existe pedido nos dois
        // sentidos" roda na Application antes de criar (ver ConnectionService).
        builder.HasIndex(c => new { c.RequesterProfileId, c.AddresseeProfileId }).IsUnique();

        // Sustenta a listagem de "pedidos recebidos" (filtro por AddresseeProfileId).
        builder.HasIndex(c => c.AddresseeProfileId);
    }
}
