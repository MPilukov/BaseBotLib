using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class ExtendedSendMessageRequest : SendMessageRequest
    {
        [DataMember(Name = "parse_mode")]
        public string ParseMode { get; set; }
    }
}