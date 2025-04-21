using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class EditMessageTextRequest : SendMessageRequest
    {
        [DataMember(Name = "message_id")]
        public string ExistMessageId { get; set; }
    }
}