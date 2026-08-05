using System.Reflection;
using FluentValidation;
using GtaConnect.Application.Features.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace GtaConnect.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
