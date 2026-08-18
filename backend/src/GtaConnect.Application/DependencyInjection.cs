using System.Reflection;
using FluentValidation;
using GtaConnect.Application.Features.Auth;
using GtaConnect.Application.Features.Chat;
using GtaConnect.Application.Features.Feed;
using GtaConnect.Application.Features.Moderation;
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
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IModerationService, ModerationService>();
        services.AddScoped<IFeedService, FeedService>();

        return services;
    }
}
