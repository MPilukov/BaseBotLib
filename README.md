# BaseBotLib
BaseBotLib is a .NET/.NET Core library for creating Telegram bots.

---
[![NuGet version](https://img.shields.io/nuget/v/BaseBotLib_Pilyukov.svg?style=flat-square)](https://www.nuget.org/packages/BaseBotLib_Pilyukov/)
[![NuGet downloads](https://img.shields.io/nuget/dt/BaseBotLib_Pilyukov?logo=nuget&label=nuget%20downloads&style=flat-square)](https://www.nuget.org/packages/BaseBotLib_Pilyukov/)
---

### Get Started

1. Create a new bot using [BotFather](https://telegram.me/BotFather) and obtain the botId and token.

2. Install the standard Nuget package into your application.
   
   ```
   CLI : dotnet add package BaseBotLib_Pilyukov
   Package Manager : Install-Package BaseBotLib_Pilyukov
   Rider / Visual Studio : Manage NuGet Packages -> Search for BaseBotLib_Pilyukov -> Install
   ```
   
---

### Example

A simple way to receive new messages and respond to them :

```csharp
using BaseBotLib.Interfaces.Bot;
using BaseBotLib.Services.Bot;

private async Task StartBot(string botId, string botToken)
{
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
}

```
The message handler function can look like this :

```csharp  
private async Task ProcessMessage(IBot bot, Message message)
{
    var msg = message.Text?.ToLower();

    switch (msg)
    {
        case "hi":
            await bot.SendMessage(message.ChatId, $"Hi {message.UserName}");
            break;
        case "bye":
            await bot.SendMessage(message.ChatId, $"Bye {message.UserName}");
            break;
    }
}
```