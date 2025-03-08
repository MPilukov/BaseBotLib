using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class InlineSelectionMenuRequest : BaseBodyRequest
    {
        [DataMember(Name = "chat_id")]
        public string ChatId { get; set; }
        
        [DataMember(Name = "text")]
        public string Text { get; set; }
        
        [DataMember(Name = "reply_markup")]
        public CreateInlineKeyboardRequest CreateInlineKeyboardRequest { get; set; }
    }
}