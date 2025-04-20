using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class DeleteInlineMenuRequest : BaseBodyRequest
    {
        [DataMember(Name = "chat_id")]
        public string ChatId { get; set; }
        
        [DataMember(Name = "message_id")]
        public string MessageId { get; set; }
    }
    
    [DataContract]
    public class EditInlineMenuRequest : DeleteInlineMenuRequest
    {
        // null to delete
        [DataMember(Name = "reply_markup")]
        public CreateInlineKeyboardRequest InlineKeyboardRequest { get; set; }
    }
}