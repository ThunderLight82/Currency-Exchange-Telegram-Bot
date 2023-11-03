using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.App;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder().
            SetBasePath(AppContext.BaseDirectory).
            AddJsonFile("appsetings.json").
            Build();

        var tBotToken = config.GetSection("BotConfiguration")["TBotToken"];
        
        var client = new TelegramBotClient(tBotToken);
        client.StartReceiving(UpdateMessage, ErrorEvent);
        
        Console.WriteLine("Bot is started...");
        
        while (true)
        {
            await Task.Delay(1000);
        }
    }
    
    private static async Task UpdateMessage(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var message = update.Message;

        if (message.Text == "Test")
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Пшел вон");
        }
    }
    
    private static async Task ErrorEvent (ITelegramBotClient tBotClient, Exception ex, CancellationToken token)
    {
        Console.WriteLine("An error occurred: " + ex.Message);
    }
}
