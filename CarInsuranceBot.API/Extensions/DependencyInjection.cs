using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Configuration;
using CarInsuranceBot.API.Infrastructure.Services;
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

            services.AddScoped<IMessageHandler, MessageHandler>();

            services.AddScoped<IMessageSender, MessageSender>();

            services.AddScoped<IAiAssistantService, OpenAiAssistantService>();

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
            services.Configure<OpenAiSettings>(configuration.GetSection("OpenAI"));

            // Add Mendee

            return services;
        }
    }
}
