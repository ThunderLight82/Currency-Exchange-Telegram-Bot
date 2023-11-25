using TelegramBot.Models;

namespace TelegramBot.Services;

public interface ICurrencyService
{ 
    Task<Currency[]> GetAllCurrencies();

    Task<decimal> ExchangeRateOperationAsync(string currencyCode, string dateString);
}