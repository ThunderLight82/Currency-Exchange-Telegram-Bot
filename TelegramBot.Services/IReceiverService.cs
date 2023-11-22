namespace TelegramBot.Services;

public interface IReceiverService
{
    public Task ReceiveAsync(CancellationToken token);
}