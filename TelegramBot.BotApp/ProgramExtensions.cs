using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramBot.Services.Interfaces;

namespace TelegramBot.BotApp;

// Background extension class with method that run loop to accept user messages  
public class ProgramExtensions : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ProgramExtensions(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var receiver = scope.ServiceProvider.GetRequiredService<IReceiverService>();

                await receiver.ReceiveAsync(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in BackgroundService - ExecuteAsync: {ex}");
                
                await Task.Delay(TimeSpan.FromSeconds(3), token);
            }
        }
    }
}