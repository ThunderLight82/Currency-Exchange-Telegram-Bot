namespace TelegramBot.Services.Interfaces;

public interface IReceiverService
{
    Task ReceiveAsync(CancellationToken token);
}