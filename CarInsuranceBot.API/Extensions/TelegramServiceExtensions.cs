using CarInsuranceBot.API.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace CarInsuranceBot.API.Extensions
{
    public static class TelegramServiceExtensions
    {
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
    }
}
