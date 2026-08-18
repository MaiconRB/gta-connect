using System.Reflection;
using FluentValidation;
using GtaConnect.Application.Features.Auth;
using GtaConnect.Application.Features.PlayerSearch;
using GtaConnect.Application.Features.Profile;
using Microsoft.Extensions.DependencyInjection;

namespace GtaConnect.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IPlayerSearchService, PlayerSearchService>();

        return services;
    }
}
