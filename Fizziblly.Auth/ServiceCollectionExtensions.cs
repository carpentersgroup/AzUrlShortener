using Microsoft.Extensions.DependencyInjection;

namespace Fizzibly.Auth
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthHandlers(this IServiceCollection services)
        {
            services.AddSingleton<TokenHandler>();
            services.AddSingleton<JwtHandler>();
            services.AddSingleton<AppOnlyTokenHandler>();
            services.AddSingleton<RoleHandler>();
            services.AddSingleton<UserHandler>();
            services.AddSingleton<StaticWebAppHandler>();
            return services;
        }

        public static IServiceCollection RegisterAuthHandlers(this IServiceCollection services)
        {
            return services.AddSingleton<HandlerContainer>(s => BuildAuthHandlers(s));
        }

        public static HandlerContainer BuildAuthHandlers(IServiceProvider serviceProvider)
        {
            IHandler rootHandler =
                serviceProvider.GetRequiredService<TokenHandler>();
            rootHandler
                .Use(serviceProvider.GetRequiredService<JwtHandler>())
                ?.Use(serviceProvider.GetRequiredService<AppOnlyTokenHandler>())
                ?.Use(serviceProvider.GetRequiredService<RoleHandler>())
                ?.Use(serviceProvider.GetRequiredService<UserHandler>());

            return new HandlerContainer(rootHandler);
        }
    }
}