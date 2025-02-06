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
        /// <summary>
        /// You cannot get new messages with GetNewMessages() method after setting webhook.
        /// </summary>
        /// <param name="webhookUrl"></param>
        /// <param name="secretToken"></param>
        /// <returns></returns>
        Task<BaseResponse> SetWebhook(string webhookUrl, string secretToken);
        Task<BaseResponse> DeleteWebhook();
    }
}