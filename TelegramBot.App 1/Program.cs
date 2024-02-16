using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Polling;
using TelegramBot.Models;
using TelegramBot.Services;
using TelegramBot.Services.Interfaces;
using TelegramBot.Services.ServiceExtensions;

namespace TelegramBot.App;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        using var host = CreateDefaultBuilder(args).Build();
        
        Console.WriteLine("\nBot is started...\n");
        
        await host.RunAsync();
    }

    private static IHostBuilder CreateDefaultBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
        {
            services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));

            services.InitializeTelegramBotClient();
            
            services.AddScoped<IUpdateHandler, UpdateHandlerService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IReceiverService, ReceiverService>();
            
            services.AddHostedService<ProgramExtensions>();
        });
}