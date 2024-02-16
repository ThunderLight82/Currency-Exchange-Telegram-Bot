using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Services;
using TelegramBot.Services.Interfaces;
using Xunit;

namespace TelegramBot.UnitTests;

public class UpdateHandlerServiceTests
{
    private readonly Mock<ITelegramBotClient> _botClient;
    private readonly Mock<ICurrencyService> _currencyServiceMock;
    private readonly Mock<IUpdateHandlerService> _updateHandlerService;

    public UpdateHandlerServiceTests()
    {
        _botClient = new Mock<ITelegramBotClient>();
        _currencyServiceMock = new Mock<ICurrencyService>();
        
        _updateHandlerService = new Mock<IUpdateHandlerService>();
    }
    
    // Не совсем валидные тесты как мне кажется, но другое я пока не придумал.
    [Fact]
    public async Task HandleUpdateAsync_MessageUpdate_CallsHandleMessageReceivedAsync()
    {
        // Arrange
        var testUpdate = new Update { Message = new Message { Text = "Some text" } };
        
        _updateHandlerService.Setup(upd =>
                upd.HandleUpdateAsync(It.IsAny<ITelegramBotClient>(), testUpdate, It.IsAny<CancellationToken>()))
            .Callback<ITelegramBotClient, Update, CancellationToken>((_, _, token) =>
                _updateHandlerService.Object.HandleMessageReceivedAsync(testUpdate.Message, token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _updateHandlerService.Object.HandleUpdateAsync(_botClient.Object, testUpdate, CancellationToken.None);
            
        // Assert
        _updateHandlerService.Verify(upd =>
            upd.HandleUpdateAsync(It.IsAny<ITelegramBotClient>(), testUpdate, It.IsAny<CancellationToken>()),
            Times.Once);
        _updateHandlerService.Verify(upd =>
                upd.HandleMessageReceivedAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _updateHandlerService.Verify(upd =>
            upd.HandleUnknownUpdate(testUpdate), Times.Never);
    }
    
    [Fact]
    public async Task HandleUpdateAsync_UnknownUpdate_CallsHandleUnknownUpdate()
    {
        // Arrange
        var testUpdate = new Update { Poll = new Poll() };
        
        _updateHandlerService.Setup(upd =>
                upd.HandleUpdateAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Update>(), It.IsAny<CancellationToken>()))
            .Callback<ITelegramBotClient, Update, CancellationToken>((_, _, _) => 
                _updateHandlerService.Object.HandleUnknownUpdate(testUpdate))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _updateHandlerService.Object.HandleUpdateAsync(_botClient.Object, testUpdate, CancellationToken.None);

        // Assert
        _updateHandlerService.Verify(upd =>
                upd.HandleUpdateAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Update>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _updateHandlerService.Verify(u =>
            u.HandleUnknownUpdate(testUpdate), Times.Once);
        _updateHandlerService.Verify(upd =>
                upd.HandleMessageReceivedAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.Never);
    } 
    
    [Fact]
    public async Task HandleMessageReceivedAsync_StartCommand_CallsStartResponseWithAdditionalCurrencyInfoAsync()
    {
        // Arrange
        var testMessage = new Message();

        _updateHandlerService.Setup(upd =>
                upd.HandleMessageReceivedAsync(testMessage, It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((_, token) =>
            { 
                _updateHandlerService.Object.StartResponseAsync(testMessage, token); 
                _updateHandlerService.Object.SupportedCurrencyListResponseAsync(testMessage, token);
            })
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _updateHandlerService.Object.HandleMessageReceivedAsync(testMessage, CancellationToken.None);

        // Assert
        _updateHandlerService.Verify(upd =>
            upd.HandleMessageReceivedAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once);
        _updateHandlerService.Verify(upd =>
            upd.StartResponseAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once);
        _updateHandlerService.Verify(upd =>
            upd.SupportedCurrencyListResponseAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once);
        _updateHandlerService.Verify(upd =>
            upd.HandleCurrencyCommandsAsync(testMessage, It.IsAny<CancellationToken>()), Times.Never);
        _updateHandlerService.Verify(upd =>
            upd.UnsupportedMessageTypeResponseAsync(testMessage, It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task HandleMessageReceivedAsync_HelpCommand_CallsStartResponseWithAdditionalCurrencyInfoAsync()
    {
        // Arrange
        var testMessage = new Message { Text = "/help"};

        _updateHandlerService.Setup(upd =>
                upd.HandleMessageReceivedAsync(testMessage, It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((_, token) =>
            { 
                _updateHandlerService.Object.StartResponseAsync(testMessage, token); 
                _updateHandlerService.Object.SupportedCurrencyListResponseAsync(testMessage, token);
            })
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _updateHandlerService.Object.HandleMessageReceivedAsync(testMessage, CancellationToken.None);

        // Assert
        _updateHandlerService.Verify(upd =>
            upd.HandleMessageReceivedAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once);
        _updateHandlerService.Verify(upd =>
            upd.StartResponseAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once);
        _updateHandlerService.Verify(upd =>
            upd.SupportedCurrencyListResponseAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once);
        _updateHandlerService.Verify(upd =>
            upd.HandleCurrencyCommandsAsync(testMessage, It.IsAny<CancellationToken>()), Times.Never);
        _updateHandlerService.Verify(upd =>
            upd.UnsupportedMessageTypeResponseAsync(testMessage, It.IsAny<CancellationToken>()), Times.Never);
    }
    
    // Попытка собрать немного иначе Аррендж. Неуспешная.
    [Fact]
    public async Task HandleMessageReceivedAsync_StartOrHelpCommand_CallsStartAndSupportedCurrencyListResponses()
    {
        // Arrange
        var startMessage = new Message { Text = "/start" };
        var helpMessage = new Message { Text = "/help" };

        _updateHandlerService.Setup(u =>
                u.HandleMessageReceivedAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        _updateHandlerService.Setup(u =>
                u.StartResponseAsync(startMessage, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        _updateHandlerService.Setup(u =>
                u.StartResponseAsync(helpMessage, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        _updateHandlerService.Setup(u =>
                u.SupportedCurrencyListResponseAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _updateHandlerService.Object.HandleMessageReceivedAsync(startMessage, default);
        await _updateHandlerService.Object.HandleMessageReceivedAsync(helpMessage, default);

        // Assert
        _updateHandlerService.Verify(u =>
            u.StartResponseAsync(startMessage, default), Times.Once);

        _updateHandlerService.Verify(u =>
            u.StartResponseAsync(helpMessage, default), Times.Once);

        _updateHandlerService.Verify(u =>
            u.SupportedCurrencyListResponseAsync(It.IsAny<Message>(), default), Times.Exactly(2));
    }
}