using Microsoft.EntityFrameworkCore;
using TelegramBot.App.Entities;

namespace TelegramBot.App.DataAccess;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base (options)
    {
    }
    
    public DbSet<AppUser> Users { get; set; }
}