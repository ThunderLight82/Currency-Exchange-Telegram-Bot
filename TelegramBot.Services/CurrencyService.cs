using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TelegramBot.Models;

namespace TelegramBot.Services;

// This service handle main operations logic with currency. 
public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _client;
    private readonly BotConfiguration _botConfiguration;
    
    public CurrencyService(IHttpClientFactory factory, IOptions<BotConfiguration> botConfiguration)
    {
        _client = factory.CreateClient("Privat24Client");
        _botConfiguration = botConfiguration.Value;
    }
    
    // Array of all supported currencies in Privat24Archive API.
    private readonly Currency[] _currencies =
    {
        new("USD", "🇺🇸"), new("EUR", "🇪🇺"), new("CHF", "🇨🇭"),
        new("GBP", "🇬🇧"), new("SEK", "🇸🇪"), new("XAU", "🟨"), 
        new("CAD", "🇨🇦"), new("AUD", "🇦🇺"), new("AZN", "🇦🇿"),
        new("BYN", "🇧🇾"), new("CNY", "🇨🇳"), new("CZK", "🇨🇿"), 
        new("DKK", "🇩🇰"), new("GEL", "🇬🇪"), new("HUF", "🇭🇺"),
        new("ILS", "🇮🇱"), new("JPY", "🇯🇵"), new("KZT", "🇰🇿"), 
        new("MDL", "🇲🇩"), new("NOK", "🇳🇴"), new("PLN", "🇵🇱"),
        new("SGD", "🇸🇬"), new("TMT", "🇹🇲"), new("TRY", "🇹🇷"),
        new("UZS", "🇺🇿")
    };
    
    public Task<Currency[]> GetAllCurrencies()
    {
        return Task.FromResult(_currencies);
    }
    
    // Core logic for getting exchange rate from .JSON by converting it.
    public async Task<decimal> ExchangeRateOperationAsync(string currencyCode, string dateString)
    {
        if (string.IsNullOrEmpty(currencyCode) || string.IsNullOrEmpty(dateString))
            throw new ArgumentNullException(string.IsNullOrEmpty(currencyCode) ?
                nameof(currencyCode) : nameof(dateString));
        try
        { 
            // Get third-party bank API url and fill with date.
            var apiUrl = $"{_botConfiguration.PrivatBankApiUrl}&date={dateString}";

            var httpResponseMessage = await _client.GetAsync(apiUrl);
            httpResponseMessage.EnsureSuccessStatusCode();
            
            var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(responseBody)!;

            if (responseObject.exchangeRate != null)
            {
                foreach (var rate in responseObject.exchangeRate)
                {
                    if (rate.currency == currencyCode)
                    {
                        return (decimal)rate.saleRateNB;
                    }
                }
            }
            
            throw new InvalidOperationException($"Exchange rate not found in archive for currency code:" +
                                                $" {currencyCode} " +
                                                "or date string was not in a correct format");
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"HTTP request failed. Date string was not in a correct format: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to parse JSON response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An unexpected error occurred: {ex.Message}", ex);
        }
    }
}