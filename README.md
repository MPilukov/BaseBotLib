# BaseBotLib
Wrapper over api telegrams for creating bots

- [NuGet Package](https://www.nuget.org/packages/BaseBotLib_Pilyukov)

Easy way to create telegram-bot :

    var bot = new Bot(botId, botToken);
    while (true)
    {
       var newMessages = await _bot.GetNewMessages();

       foreach (var newMessage in newMessages)
       {
           _logger.Info($"New message : \"{newMessage.Text}\" from user \"{newMessage.UserName}\".");
           await ProcessMessage(newMessage);
       }
    }
  
Function-handler for message can have view :
  
    var msg = message.Text?.ToLower();
    swith (msg)
    {
       case "hi" :
         return _bot.SendMessage(message.ChatId.ToString(), $"Hi {message.UserName}");
    }
