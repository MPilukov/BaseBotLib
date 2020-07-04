using System.Threading.Tasks;

namespace BaseBotLib.Interfaces.Bot
{
    public interface IBot
    {
        Task<string> GetBotName();
        Task<Message[]> GetNewMessages();
        Task SendMessage(string chatId, string text);
        Task CreateKeyboard(string chatId, string text, string[] texts);
        Task CreateKeyboard(string chatId, string text, string[] texts, bool oneTime);
        Task CreateKeyboard(string chatId, string text, string[] texts, bool oneTime, bool resizeKeyboard);
        Task CreateInlineKeyboard(string chatId, string text, string[] texts);
        Task CreateInlineKeyboard(string chatId, string text, string[] texts, bool oneTime);
        Task CreateInlineKeyboard(string chatId, string text, string[] texts, bool oneTime, bool resizeKeyboard);
    }
}
