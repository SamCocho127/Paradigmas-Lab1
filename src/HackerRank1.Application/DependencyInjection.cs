using HackerRank1.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HackerRank1.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddTransient<ILibrariesService, LibrariesService>();
        services.AddTransient<IBooksService, BooksService>();

        return services;
    }
}
