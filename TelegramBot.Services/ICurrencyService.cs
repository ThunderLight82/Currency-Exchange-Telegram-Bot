using TelegramBot.Models;

namespace TelegramBot.Services;

public interface ICurrencyService
{
    public Task<Currency[]> GetAllCurrencies();

    public Task<decimal> GetExchangeRateAsync(string currencyCode, DateTime? dateTime);
}