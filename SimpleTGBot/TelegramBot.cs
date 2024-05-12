namespace SimpleTGBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Utils;
using System.Text.RegularExpressions;
using System.Net;


public class TelegramBot

{
    // Токен TG-бота. Можно получить у @BotFather
    private const string BotToken = "7032654041:AAFB7ywqjlEdQFd7tvWkgwXljm4l9AHUaM4";
    private States currentState;
    private HashSet<string> commands = new HashSet<string>() { "/start", "/about", "/getUserInformation", "/getRandomContest", "/getLatestActiveBlog" };
    /// <summary>
    /// Инициализирует и обеспечивает работу бота до нажатия клавиши Esc
    /// </summary>
    public async Task Run()
    {
        // Если вам нужно хранить какие-то данные во время работы бота (массив информации, логи бота,
        // историю сообщений для каждого пользователя), то это всё надо инициализировать в этом методе.
        // TODO: Инициализация необходимых полей
        currentState = States.WaitingForCommand;

        // Инициализируем наш клиент, передавая ему токен.
        var botClient = new TelegramBotClient(BotToken);

        // Служебные вещи для организации правильной работы с потоками
        using CancellationTokenSource cts = new CancellationTokenSource();

        // Разрешённые события, которые будет получать и обрабатывать наш бот.
        // Будем получать только сообщения. При желании можно поработать с другими событиями.
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        // Привязываем все обработчики и начинаем принимать сообщения для бота
        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        // Проверяем что токен верный и получаем информацию о боте
        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");

        // Ждём, пока будет нажата клавиша Esc, тогда завершаем работу бота
        while (Console.ReadKey().Key != ConsoleKey.Escape) { }

        // Отправляем запрос для остановки работы клиента.
        cts.Cancel();
    }

    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
        new KeyboardButton[] { "/start","/about", },
        new KeyboardButton[] { "/getUserInformation", "/getRandomContest","/getLatestActiveBlog" },
         })
        {
            ResizeKeyboard = true
        };



        var message = update.Message;
        if (message is null)
        {
            return;
        }



        //  Utils.Utils.PrintData(message.Text);

        /* if (message.Text is not { } messageText) // if is null
         {
             Console.WriteLine($"{message.Type}!!!!");
             return;
         }*/

        long chatId = message.Chat.Id;


        if (currentState == States.WaitingForHandle)
        {
            if (message.Text.StartsWith("/"))
            {
                await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "It's a command, not a handle😡😡😡😡🤬🤬🤬",
               replyMarkup: replyKeyboardMarkup,
               cancellationToken: cancellationToken);


                if (!commands.Contains(message.Text))
                {
                    await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: "And I don't even know this command🥱🥱🥱",
              replyMarkup: replyKeyboardMarkup,
              cancellationToken: cancellationToken);
                    currentState = States.WaitingForCommand;
                }

                currentState = States.WaitingForCommand;
                return;
            }

            string info = Utils.GetData(message.Text).Result;

            if (info == "Failed to find user")
            {
                await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: "Failed to find user",
              replyMarkup: replyKeyboardMarkup,
              cancellationToken: cancellationToken);
                currentState = States.WaitingForCommand;
                return;
            }

            string handle = "Handle: " + Regex.Match(info, "\\\"handle\\\":\\\"(.+?)\\\"").Groups[1].Value + "\n";
            string firstName;
            if (Regex.Match(info, "\\\"firstName\\\":\\\"(.+?)\\\"").Groups[1].Value != "")
            {
                firstName = "First name: " + Regex.Match(info, "\\\"firstName\\\":\\\"(.+?)\\\"").Groups[1].Value + "\n";
            }
            else
            {
                firstName = "First name: Blank\n";
            }

            string lastName;
            if (Regex.Match(info, "\\\"lastName\\\":\\\"(.+?)\\\"").Groups[1].Value != "")
            {
                lastName = "Last name: " + Regex.Match(info, "\\\"lastName\\\":\\\"(.+?)\\\"").Groups[1].Value + "\n";
            }
            else
            {
                lastName = "Last name: Blank\n";
            }

            string rating = "Rating: " + Regex.Match(info, "\\\"rating\\\":([0-9]+)").Groups[1].Value + "\n";
            string maxRating = "Max. rating: " + Regex.Match(info, "\\\"maxRating\\\":([0-9]+)").Groups[1].Value + "\n";
            string friendOf = "Friend of " + Regex.Match(info, "\\\"friendOfCount\\\":([0-9]+)").Groups[1].Value + " users\n";
            string profileUrl = "Profile url: " + $"https://codeforces.com/profile/{Regex.Match(info, "\\\"handle\\\":\\\"(.+?)\\\"").Groups[1].Value}" + "\n";
            string pictureUrl = Regex.Match(info, "\\\"titlePhoto\\\":\\\"(.+?)\\\"").Groups[1].Value + "\n";

            string toSend = handle + profileUrl + firstName + lastName + rating + maxRating + friendOf;

            await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: new InputFileUrl(pictureUrl),
                        caption: toSend,
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);

            currentState = States.WaitingForCommand;
            return;
        }



        var messageText = message.Text;
        if (messageText is null)
            return;
        if (messageText.CompareTo("/start") == 0)
        {

            await botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: "Welcome!\nPossible commands:\n/start\n/about\n/getUserInformation\n/getRandomContest\n/getLatestActiveBlog",
                 replyMarkup: replyKeyboardMarkup,
                 cancellationToken: cancellationToken);
        }
        else if (messageText.CompareTo("/getUserInformation") == 0)
        {
            currentState = States.WaitingForHandle;
            await botClient.SendTextMessageAsync(
     chatId: chatId,
     text: "Enter your Codeforces handle",
     replyMarkup: replyKeyboardMarkup,
     cancellationToken: cancellationToken);

        }
        else if (messageText.CompareTo("/getRandomContest") == 0)
        {
            Random r = new Random();


            await botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: $"https://codeforces.com/contest/{r.Next(1, 1971)}",
                 replyMarkup: replyKeyboardMarkup,
                 cancellationToken: cancellationToken);
        }
        else if (messageText.CompareTo("/getLatestActiveBlog") == 0)
        {

            string res = Utils.GetLastActiveBlogId().Result;


            await botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: res != "Failed" ? $"https://codeforces.com/blog/entry/{res}" : "Failed",
                 replyMarkup: replyKeyboardMarkup,
                 cancellationToken: cancellationToken);
        }
        else if (messageText.CompareTo("/about") == 0)
        {
            await botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: System.IO.File.ReadAllTextAsync("about.txt").Result,
                 replyMarkup: replyKeyboardMarkup,
                 cancellationToken: cancellationToken);

        }
        else
        {
            await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "I don't understand the command\nPossible commands:\nstart\nabout\ngetUserInformation\ngetRandomContest\ngetLatestActiveBlog",
               replyMarkup: replyKeyboardMarkup,
               cancellationToken: cancellationToken);
        }

    }

    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // В зависимости от типа исключения печатаем различные сообщения об ошибке
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",

            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);

        // Завершаем работу
        return Task.CompletedTask;
    }
}