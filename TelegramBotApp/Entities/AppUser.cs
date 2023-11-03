using System.ComponentModel.DataAnnotations;

namespace TelegramBot.App.Entities;

public class AppUser : BaseEntity
{
    [Key]
    public long ChatId { get; set; }

    public string? Username { get; set; }
    
    public string? FirstName { get; set; } 
    
    public string? LastName { get; set; }
}