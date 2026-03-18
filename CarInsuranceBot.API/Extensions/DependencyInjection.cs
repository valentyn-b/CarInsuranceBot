using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Configuration;
using CarInsuranceBot.API.Infrastructure.Interfaces;
using CarInsuranceBot.API.Infrastructure.Services;
using CarInsuranceBot.API.Infrastructure.Storage;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Telegram.Bot;

namespace CarInsuranceBot.API.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IUserSessionStorage, InMemoryUserSessionStorage>();

            services.AddScoped<IMessageHandler, MessageHandler>();

            services.AddScoped<IMessageService, MessageService>();

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

            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<OpenAiSettings>>().Value;
                return new ChatClient(model: "gpt-4o-mini", apiKey: settings.ApiKey);
            });

            services.AddScoped<IAiAssistantService, OpenAiAssistantService>();

            services.Configure<MindeeApiSettings>(configuration.GetSection("Mindee"));

            services.AddScoped<IDocumentRecognitionService, MockDocumentRecognitionService>();

            return services;
        }
    }
}
