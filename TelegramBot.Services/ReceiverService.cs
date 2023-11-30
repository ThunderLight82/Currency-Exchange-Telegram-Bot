using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Services;

// This service handles the reception of incoming messages from users.
public class ReceiverService : IReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandler;

    public ReceiverService(ITelegramBotClient botClient, IUpdateHandler updateHandler)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
    }
    
    public async Task ReceiveAsync(CancellationToken token)
    {
        try
        {
            var receiverOption = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                ThrowPendingUpdates = true
            };
            
            await _botClient.ReceiveAsync(_updateHandler, receiverOption, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred in ReceiverService - ReceiveAsync: {ex}");
        }
    }
}