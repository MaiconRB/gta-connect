using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Infrastructure.Auth;
using GtaConnect.Infrastructure.Identity;
using GtaConnect.Infrastructure.Persistence;
using GtaConnect.Infrastructure.Persistence.Repositories;
using GtaConnect.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GtaConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Espelha exatamente as regras do RegisterRequestValidator (Application/Features/Auth).
                // O Identity roda sua própria validação de senha antes do CreateAsync — manter as duas
                // em sincronia evita a UX ruim de passar na validação da API e falhar "por dentro" do Identity.
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IPlayerProfileRepository, PlayerProfileRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IBlockRepository, BlockRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IFeedRepository, FeedRepository>();
        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPhotoStorageService, LocalDiskPhotoStorageService>();

        return services;
    }
}
