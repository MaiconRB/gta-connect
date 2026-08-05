using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GtaConnect.Infrastructure.Persistence;

/// <summary>
/// Usada apenas pelas ferramentas de design-time do EF Core (`dotnet ef migrations`/`database update`).
/// Existe porque bootar o host completo da Api (com Identity, DataProtection, etc) só para gerar
/// uma migration é frágil — esta factory monta um DbContext mínimo, lendo a connection string
/// diretamente do appsettings + user-secrets do projeto Api, sem depender do Program.cs.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "GtaConnect.Api"))
            .AddJsonFile("appsettings.json")
            .AddUserSecrets("85586417-f863-4fc0-8fdf-a97d04424f4d")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada para design-time.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
