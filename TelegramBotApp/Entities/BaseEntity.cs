namespace TelegramBot.App.Entities;

public class BaseEntity
{
    public long UserId { get; set; }

    public DateTime WhenIsCreated { get; set; } = DateTime.UtcNow;
}