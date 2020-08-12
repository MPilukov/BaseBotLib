# BaseBotLib
Wrapper over api telegrams for creating bots

- [NuGet Package](https://www.nuget.org/packages/BaseBotLib_Pilyukov)

Easy way to create telegram-bot :

    var bot = new Bot(botId, botToken);
    while (true)
    {
        var newMessages = await bot.GetNewMessages();

        foreach (var newMessage in newMessages)
        {
            Console.WriteLine($"New message : \"{newMessage.Text}\" from user \"{newMessage.UserName}\".");
            await ProcessMessage(bot, newMessage);
        }

        await Task.Delay(20);
    }
  
Function-handler for message can have view :
  
    private static async Task ProcessMessage(IBot bot, Message message)
    {
        var msg = message.Text?.ToLower();

        switch (msg)
        {
            case "hi":
                await bot.SendMessage(message.ChatId.ToString(), $"Hi {message.UserName}");
                break;
            case "bye":
                await bot.SendMessage(message.ChatId.ToString(), $"Bye {message.UserName}");
                break;
        }
    }
