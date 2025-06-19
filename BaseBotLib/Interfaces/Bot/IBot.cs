using System;
using System.Threading.Tasks;
using BaseBotLib.Interfaces.Bot.Menu;

namespace BaseBotLib.Interfaces.Bot
{
    public interface IBot
    {
        Task<string> GetBotName();
        Task<string> GetBotUserName();
        Task<Message[]> GetNewMessages();
        Task<SendMessageBaseResponse> SendMessage(string chatId, string text);
        
        /// <summary>
        /// 10 MB max size for photos, 50 MB for other files.
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="fileName"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<SendFileBaseResponse> SendPhoto(string chatId, string fileName, byte[] body);

        /// <summary>
        /// 10 MB max size for photos, 50 MB for other files.
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="fileName"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<SendFileBaseResponse> SendFile(string chatId, string fileName, byte[] body);
        Task<BaseResponse> EditMessage(string chatId, string messageId, string text);
        Task<BaseResponse> DeleteMessage(string chatId, string messageId);
        Task<SendMessageBaseResponse> SendMessageWithMarkdown(string chatId, string text);
        Task<SendMessageBaseResponse> SendMessageWithHtml(string chatId, string text);
        
        // todo: send file/photo
        Task<BaseResponse> SendSelectionMenu(string chatId, SelectionMenu menu);
        Task<SendMessageBaseResponse> SendInlineSelectionMenu(string chatId, InlineSelectionMenu menu);
        Task<BaseResponse> EditInlineSelectionMenu(string chatId, string messageId, InlineSelectionMenu menu);
        Task<byte[]> GetFile(string fileId);
        
        /// <summary>
        /// You cannot use the GetNewMessages() method after setting a webhook.
        /// </summary>
        /// <param name="webhookUrl"></param>
        /// <param name="secretToken"></param>
        /// <returns></returns>
        Task<BaseResponse> SetWebhook(string webhookUrl, string secretToken);
        Task<BaseResponse> DeleteWebhook();
        Task<Webhook> GetWebhooks();
        
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