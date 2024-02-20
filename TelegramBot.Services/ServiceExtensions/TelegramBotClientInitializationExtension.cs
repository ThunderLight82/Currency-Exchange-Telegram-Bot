using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramBot.Models;

namespace TelegramBot.Services.ServiceExtensions;

public static class TelegramBotClientInitializationExtension
{
    public static void InitializeTelegramBotClient(this IServiceCollection services)
    {
        services.AddHttpClient("TelegramBotClient").AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
        {
            var botClientConfig = sp.GetService<IOptions<BotConfiguration>>()?.Value;
            
            if (string.IsNullOrWhiteSpace(botClientConfig?.BotToken))
                Console.WriteLine("Bot token is missing or empty. Please, check the configuration.");

            return new TelegramBotClient(new TelegramBotClientOptions(botClientConfig?.BotToken!), httpClient);
        });
    }
}