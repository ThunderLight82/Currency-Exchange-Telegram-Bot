using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Models;
using TelegramBot.Services.Interfaces;

namespace TelegramBot.Services;

// This service is responsible for handling incoming updates from the Telegram Bot API and processing messages from users. 
public class UpdateHandlerService : IUpdateHandlerService, IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ICurrencyService _currencyService;

    public UpdateHandlerService(ITelegramBotClient botClient, ICurrencyService currencyService)
    {
        _botClient = botClient;
        _currencyService = currencyService;
    }

    #region Handlers
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                await HandleMessageReceivedAsync(update.Message, token);
                break;

            default:
                await HandleUnknownUpdate(update);
                break;
        }
    }
    
    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Polling Error: {exception}");

        switch (exception)
        {
            // Handle network-related errors
            case HttpRequestException:
                Console.WriteLine("Network error. Check your internet connection.");
                
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                break;

            // Handle Telegram API errors
            case ApiRequestException apiErrorException:
                Console.WriteLine($"Telegram API error: {apiErrorException.ErrorCode} - {apiErrorException.Message}");

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                break;
        }
    }
    
    public Task HandleUnknownUpdate(Update update)
    {
        Console.WriteLine($"Received an unknown update type: {update.Type}");
        
        return Task.CompletedTask;
    }

    // Handles different input messages.
    public async Task HandleMessageReceivedAsync(Message message, CancellationToken token)
    {
        if (message.Type == MessageType.Text)
        {
            var userMessage = message.Text;

            switch (userMessage)
            {
                // Handle the [/start] and [/help] inputs.
                case "/start":
                case "/help":
                    await StartResponseAsync(message, token);
                    await SupportedCurrencyListResponseAsync(message, token);
                    break;

                // Handle exchange rate inputs if user message contain any currency code.
                default:
                    await HandleCurrencyCommandsAsync(message, token);
                    break;
            }
        }
        // Handle case when message type is not text or null.
        else
        {
            await UnsupportedMessageTypeResponseAsync(message, token);
        }
    }
    
    public async Task HandleCurrencyCommandsAsync(Message message, CancellationToken token)
    {
        // Parse and convert user message to uppercase because further exchange logic.
        var userMessage = message.Text?.ToUpper();
        
        var (currencyNameCode, dateTime) = ParseUserMessageWithCurrencyAndDate(userMessage);

        await GetAndSendExchangeRateResponseAsync(message, currencyNameCode,
            dateTime, _currencyService.ExchangeRateOperationAsync, token);
    }
    
    #endregion

    #region ResponseMessages

    // Response [/start] input.
    public async Task StartResponseAsync(Message message, CancellationToken token)
    {
        await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing,
            cancellationToken: token);

        var sendMessage =
            "*Welcome to @currency_exchange_by_olegp_bot!*" +
            "*\n\nHow it's works:\n*" +
            "You can obtain the exchange rate from a foreign currency to *Ukrainian Hryvnia (UAH 🇺🇦)* by entering a preferred currency code and a date in your message.\n" +
            "Alternatively, you can simply type the currency code to get the current up-to-date exchange rate. \n\n" +
            "*Example:* \n" +
            "*1.* \"USD\" or \"PLN\" — get today exchange rate.\n" +
            "*2.* \"USD 23.03.2020\" or \"USD 23/03/2020\" — get specific date exchange rate.";

        await _botClient.SendTextMessageAsync(message.Chat.Id, sendMessage, parseMode: ParseMode.Markdown,
            cancellationToken: token);
    }

    // Another message right after [/start] message for additional help.
    public async Task SupportedCurrencyListResponseAsync(Message message, CancellationToken token)
    {
        var currencyList = await _currencyService.GetAllCurrencies();

        var listSeparation = currencyList.Length / 2;

        var firstColumn = currencyList.Take(listSeparation);
        var secondColumn = currencyList.Skip(listSeparation);

        var combinedRows = firstColumn.Zip(secondColumn, (first, second) =>
            $"*{first.CurrencyNameCode,-1}* {first.CurrencyEmoji,-20}  *{second.CurrencyNameCode,-1}* {second.CurrencyEmoji,-20}\n");

        var sendMessage = $"Supported list of currencies with appropriate code:\n\n{string.Join("", combinedRows)}";

        await _botClient.SendTextMessageAsync(message.Chat.Id, sendMessage, parseMode: ParseMode.Markdown,
            cancellationToken: token);
    }
    
    // Response to non-text input messages.
    public async Task UnsupportedMessageTypeResponseAsync(Message message, CancellationToken token)
    {
        var unsupportedMessageType = "*Unsupported message type. *" + 
                                     "*Please, send a valid message that contains currency code with or without date.*";
        
        await _botClient.SendTextMessageAsync(message.Chat.Id,unsupportedMessageType, parseMode: ParseMode.Markdown,
            cancellationToken: token);
    }
    
    // Retrieves the exchange rate and sends the result as a message.
    private async Task GetAndSendExchangeRateResponseAsync(Message message, string currencyNameCode,
            DateTime? dateTime, Func<string, string, Task<decimal>> exchangeRateFunction, CancellationToken token)
    {
        var getCurrencies = await _currencyService.GetAllCurrencies();
        
        if (CurrencyCodeIsValid(currencyNameCode, getCurrencies))
        {
            if (DateIsWithinBankArchiveRange(dateTime.Value))
            {
                decimal exchangeRate = await exchangeRateFunction(currencyNameCode, GetDateString(dateTime));
            
                var currency = getCurrencies.FirstOrDefault(c => c.CurrencyNameCode == currencyNameCode);

                var sendMessage =
                    $"Exchange Rate from *{currency.CurrencyNameCode}* {currency.CurrencyEmoji} to" +
                    $" *UAH* 🇺🇦 is *{exchangeRate}* by *{dateTime:dd.MM.yyyy}*";

                await _botClient.SendTextMessageAsync(message.Chat.Id, sendMessage, parseMode: ParseMode.Markdown,
                    cancellationToken: token);
            }
            else
            {
                var dateOutOfRangeErrorMessage =
                    "Invalid date. The provided date is beyond the allowed range.\n" +
                    "Please, use a date within the last *7 years* from today.";
                
                await _botClient.SendTextMessageAsync(message.Chat.Id, dateOutOfRangeErrorMessage, parseMode: ParseMode.Markdown,
                    cancellationToken: token);
            }
            
        }
        else
        {
            var invalidDateFormatErrorMessage =
                "Invalid date format or currency code. Please, use the format:\n\n" +
                "1. *[CurrencyCode]* - for today's date.\n" +
                "2. *[CurrencyCode]* *[DD.MM.YYYY or DD/MM/YYYY]* - for specific date.\n\n" +
                "/help for addition info about currencies and exchange.";
            
            await _botClient.SendTextMessageAsync(message.Chat.Id, invalidDateFormatErrorMessage,
                parseMode: ParseMode.Markdown,
                cancellationToken: token);
        }
    }

    #endregion

    #region Other

    // Date parsing from users messages. If no date is specified or it's typed inaccurate in message - set today's date instead of throwing error. 
    private (string currencyCode, DateTime? date) ParseUserMessageWithCurrencyAndDate(string userMessage)
    {
        // Regex format: [CurrencyCode] [Optional: (DD.MM.YYYY) or (DD/MM/YYYY)]
        var regex = new Regex(@"^(\w+)\s*(\d{2}[./]\d{2}[./]\d{4})?$");
        var match = regex.Match(userMessage);

        return match.Success switch
        {
            true when match.Groups.Count == 3 && DateTime.TryParseExact(match.Groups[2].Value, new[]
                        { "dd.MM.yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var parsedDate) => (match.Groups[1].Value, parsedDate),

            true => (match.Groups[1].Value, DateTime.Now.Date),
            
            _ => (string.Empty, null)
        };
    }

    private static bool CurrencyCodeIsValid(string currencyCode, IEnumerable<Currency> currencies)
    {
        return currencies.Any(c => c.CurrencyNameCode == currencyCode);
    }

    // Check if the date is within the valid range. Bank api holds up to 7-9 years of data from today's date, but I use 7.
    private static bool DateIsWithinBankArchiveRange(DateTime dateTime)
    {
        var currentDate = DateTime.Now;
        var validDateCheck = currentDate.AddYears(-7);

        return dateTime >= validDateCheck && dateTime <= currentDate;
    }

    private static string GetDateString(DateTime? dateTime)
    {
        return dateTime?.ToString("dd.MM.yyyy") ?? DateTime.Now.ToString("dd.MM.yyyy");
    }
    
    #endregion
}