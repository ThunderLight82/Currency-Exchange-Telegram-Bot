namespace TelegramBot.Models;

public class Currency
{
    public Currency(string nameCode, string emoji)
    {
        CurrencyNameCode = nameCode;
        CurrencyEmoji = emoji;
    }
    
    public string CurrencyNameCode { get; }

    public string CurrencyEmoji { get; }
    
    public override string ToString()
    {
        return $"{CurrencyNameCode} {CurrencyEmoji}";
    }
}