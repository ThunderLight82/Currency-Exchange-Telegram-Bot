namespace TelegramBot.App.Models;

public class Currency
{
    public Currency(string nameCode, string emoji)
    {
        CurrencyNameCode = nameCode;
        CurrencyEmoji = emoji;
    }
    
    public string CurrencyNameCode { get; set; }

    public string CurrencyEmoji { get; set; }
    
    public override string ToString()
    {
        return $"{CurrencyNameCode} {CurrencyEmoji}";
    }
}