using System.Threading.Tasks;

namespace BaseBotLib.Interfaces.Bot
{
    public interface IBot
    {
        Task<string> GetBotName();
        Task<Message[]> GetNewMessages();
        Task SendMessage(string chatId, string text);
        Task AnswerCallbackQuery(string id, string text, int cacheTime, bool showAlert);
    }
}
