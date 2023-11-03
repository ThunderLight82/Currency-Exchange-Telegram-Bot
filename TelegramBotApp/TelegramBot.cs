using Telegram.Bot;

namespace TelegramBot.App;

public class TelegramBot
{
    private readonly IConfiguration _configuration;
    private TelegramBotClient _telegramBotClient;

    public TelegramBot(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<TelegramBotClient> GetBot()
    {
        if (_telegramBotClient != null)
            return _telegramBotClient;
        
        _telegramBotClient = new TelegramBotClient(_configuration["TBotToken"]);

        var hook = $"{_configuration["Url"]} api/message/update";
        
        await _telegramBotClient.SetWebhookAsync(hook);

        return _telegramBotClient;
    }
}