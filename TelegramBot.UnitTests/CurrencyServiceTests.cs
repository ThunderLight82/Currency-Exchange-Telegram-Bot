using Microsoft.Extensions.Options;
using Moq;
using TelegramBot.Models;
using TelegramBot.Services;
using Xunit;

namespace TelegramBot.UnitTests;

public class CurrencyServiceTests
{
    private readonly ICurrencyService _currencyService;

    public CurrencyServiceTests()
    {
        var httpClient = new HttpClient();
        
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var botConfigurationMock = new Mock<IOptions<BotConfiguration>>();
        botConfigurationMock.SetupGet(c => c.Value).Returns(new BotConfiguration
        {
            PrivatBankApiUrl = "https://api.privatbank.ua/p24api/exchange_rates?json"
        });

        _currencyService = new CurrencyService(httpClientFactoryMock.Object, botConfigurationMock.Object);
    }
    
    [Theory]
    [InlineData("USD", "20.11.2023", 36.14)]
    [InlineData("EUR", "23.03.2020", 29.64)]
    [InlineData("EUR", "01.01.2020", 26.42)]
    [InlineData("PLN", "31.12.2017", 8.01)]
    [InlineData("NOK", "10.10.2017", 3.33)]
    public async Task GetExchangeRateAsync_AllValidCurrencyAndDate_ReturnExpectedExchangeResult(
        string currencyNameCode, string dateString, decimal expectedExchangeRateResult)
    {
        // Arrange & Act 
        var exchangeResult = await _currencyService.ExchangeRateOperationAsync(currencyNameCode, dateString);

        // Assert
        Assert.Equal(expectedExchangeRateResult, exchangeResult, precision: 2);
    }
    
    // Скорее всего абстрактный тест. Огромная часть если не все эти ошибки никогда не будет закетчены в при реальной работе бота.
    // UpdateHandlerService не даст большой части их дойти в этот метод, но оставлю их все-равно на всякий.
    [Theory]
    [InlineData("USD", "99.99.2099", typeof(HttpRequestException))]
    [InlineData("EUR", "00.01.0120", typeof(HttpRequestException))]
    [InlineData("JPY", " 10. 01. 2019", typeof(HttpRequestException))]
    [InlineData("JPY", "10-01-2019", typeof(HttpRequestException))]
    [InlineData("JPY", "-10-01-2019-", typeof(HttpRequestException))]
    [InlineData("JPY", "10,01,2019", typeof(HttpRequestException))]
    [InlineData("CAD", "01.01.2025", typeof(InvalidOperationException))]
    [InlineData("EUR!", "23.03.2020", typeof(InvalidOperationException))]
    [InlineData("cAD", "01.01.2020", typeof(InvalidOperationException))]
    [InlineData("  c  A D   ", "01.01.2020", typeof(InvalidOperationException))]
    [InlineData("1  EUR   ", "23.03.2020", typeof(InvalidOperationException))]
    [InlineData("PLNJPYUSD", "23.03.2020", typeof(InvalidOperationException))]
    [InlineData(null, "23.03.2020", typeof(ArgumentNullException))]
    [InlineData("0354g435eg g 4eg 4", null, typeof(ArgumentNullException))]
    [InlineData("", null, typeof(ArgumentNullException))]
    [InlineData(" ", null, typeof(ArgumentNullException))]
    [InlineData(null, "", typeof(ArgumentNullException))]
    [InlineData(null, " ", typeof(ArgumentNullException))]
    [InlineData(null, null, typeof(ArgumentNullException))]
    public async Task? GetExchangeRateAsync_InvalidNullEmptyCurrencyOrDate_ReturnExpectedException(
        string currencyNameCode, string dateString,  Type expectedExceptionType)
    {
        // Arrange & Act 
        var exchangeException = await Record.ExceptionAsync(() =>
            _currencyService.ExchangeRateOperationAsync(currencyNameCode, dateString));

        // Assert
        Assert.NotNull(exchangeException);
        Assert.IsType(expectedExceptionType, exchangeException);
    }
}