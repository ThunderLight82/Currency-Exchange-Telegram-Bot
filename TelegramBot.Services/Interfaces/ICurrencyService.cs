using TelegramBot.Models;

namespace TelegramBot.Services.Interfaces;

public interface ICurrencyService
{ 
    Task<Currency[]> GetAllCurrencies();
    Task<decimal> ExchangeRateOperationAsync(string currencyCode, string dateString);
}