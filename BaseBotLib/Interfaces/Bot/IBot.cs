using System;
using System.Threading.Tasks;
using BaseBotLib.Interfaces.Bot.Menu;

namespace BaseBotLib.Interfaces.Bot
{
    public interface IBot
    {
        Task<string> GetBotName();
        Task<Message[]> GetNewMessages();
        Task SendMessage(string chatId, string text);
        
        // todo: send file/photo
        Task<BaseResponse> SendSelectionMenu(string chatId, SelectionMenu menu);
        Task<BaseResponse> SendInlineSelectionMenu(string chatId, InlineSelectionMenu menu);
        Task<byte[]> GetFile(string fileId);
        
        /// <summary>
        /// You cannot use the GetNewMessages() method after setting a webhook.
        /// </summary>
        /// <param name="webhookUrl"></param>
        /// <param name="secretToken"></param>
        /// <returns></returns>
        Task<BaseResponse> SetWebhook(string webhookUrl, string secretToken);
        Task<BaseResponse> DeleteWebhook();
        
        [Obsolete]
        Task CreateKeyboard(string chatId, string textMenu, string[] textButtons);
        
        [Obsolete]
        Task CreateKeyboard(string chatId, string textMenu, string[] textButtons, bool oneTime);
        
        [Obsolete]
        Task CreateKeyboard(string chatId, string textMenu, string[] textButtons, bool oneTime, bool resizeKeyboard);
        
        [Obsolete]
        Task CreateInlineKeyboard(string chatId, string textMenu, string[] textButtons);
        
        [Obsolete]
        Task CreateInlineKeyboard(string chatId, string textMenu, string[] textButtons, bool oneTime);
        
        [Obsolete]
        Task CreateInlineKeyboard(string chatId, string textMenu, string[] textButtons, bool oneTime, bool resizeKeyboard);
    }
}