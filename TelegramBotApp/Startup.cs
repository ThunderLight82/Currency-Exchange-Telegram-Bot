using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBot.App.DataAccess;

namespace TelegramBot.App;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddDbContext<DataContext>(option =>
            option.UseSqlServer(_configuration.GetConnectionString("TelegramBotDB")));
        services.AddSingleton<TelegramBot>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment whBuilder)
    {
        if (whBuilder.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoint =>
        { 
            endpoint.MapControllers();
        });
    }
}