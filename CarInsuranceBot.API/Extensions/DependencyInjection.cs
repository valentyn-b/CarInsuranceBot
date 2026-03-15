using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Configuration;
using CarInsuranceBot.API.Infrastructure.Storage;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace CarInsuranceBot.API.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IUserStateStorage, InMemoryUserStateStorage>();

            return services;
        }

        public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TelegramSettings>(configuration.GetSection("TelegramSettings"));

            services.AddSingleton<ITelegramBotClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<TelegramSettings>>().Value;
                return new TelegramBotClient(settings.BotToken);
            });

            return services;
        }

        public static IServiceCollection AddExternalIntegrations (this IServiceCollection services, IConfiguration configuration)
        {
            // Add OpenAI

            // Add Mendee

            return services;
        }
    }
}
