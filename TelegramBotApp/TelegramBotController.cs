using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.App.DataAccess;
using TelegramBot.App.Entities;

namespace TelegramBot.App;

[ApiController]
[Route("api/message")]

public class TelegramBotController : ControllerBase
{
    private readonly TelegramBotClient _telegramBotClient;
    private readonly DataContext _dbContext;

    public TelegramBotController(TelegramBot telegramBot, DataContext dbContext)
    {
        _dbContext = dbContext;
        _telegramBotClient = telegramBot.GetBot().Result;
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody]object update)
    {
        // when "/start" => register new user 

        var upd = JsonConvert.DeserializeObject<Update>(update.ToString());
        var chat = upd.Message?.Chat;

        if (chat == null)
            return Ok();
        
        var appUser = new AppUser
        {
            Username = chat.Username,
            ChatId = chat.Id,
            FirstName = chat.FirstName,
            LastName = chat.LastName
        };

        await _dbContext.Users.AddAsync(appUser);
        
        await _dbContext.SaveChangesAsync();

        await _telegramBotClient.SendTextMessageAsync(chat.Id, "Registration Completed!");
        
        return Ok();
    }
}