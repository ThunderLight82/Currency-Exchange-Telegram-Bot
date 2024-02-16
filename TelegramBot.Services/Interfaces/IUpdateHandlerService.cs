using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Services.Interfaces;

public interface IUpdateHandlerService
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token);
    
    Task HandleUnknownUpdate(Update update);
    
    Task HandleMessageReceivedAsync(Message message, CancellationToken token);

    Task StartResponseAsync(Message message, CancellationToken token);

    Task SupportedCurrencyListResponseAsync(Message message, CancellationToken token);
    
    Task HandleCurrencyCommandsAsync(Message message, CancellationToken token);

    Task UnsupportedMessageTypeResponseAsync(Message message, CancellationToken token);
}