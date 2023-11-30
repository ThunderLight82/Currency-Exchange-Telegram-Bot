namespace TelegramBot.Services;

public interface IReceiverService
{
    Task ReceiveAsync(CancellationToken token);
}