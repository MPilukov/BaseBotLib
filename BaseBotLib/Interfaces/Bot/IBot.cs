using System.Threading.Tasks;

namespace BaseBotLib.Interfaces.Bot
{
    public interface IBot
    {
        Task<string> GetBotName();
        Task<Message[]> GetNewMessages();
        Task SendMessage(string chatId, string text);
        Task CreateKeyboard(string chatId, string textMenu, string[] textButtons);
        Task CreateKeyboard(string chatId, string textMenu, string[] textButtons, bool oneTime);
        Task CreateKeyboard(string chatId, string textMenu, string[] textButtons, bool oneTime, bool resizeKeyboard);
        Task CreateInlineKeyboard(string chatId, string textMenu, string[] textButtons);
        Task CreateInlineKeyboard(string chatId, string textMenu, string[] textButtons, bool oneTime);
        Task CreateInlineKeyboard(string chatId, string textMenu, string[] textButtons, bool oneTime, bool resizeKeyboard);
        Task<byte[]> GetFile(string fileId);
        
        // https://core.telegram.org/bots/api#setwebhook
        Task SetWebhook(string url);
        Task DeleteWebhook(string url);
    }
}