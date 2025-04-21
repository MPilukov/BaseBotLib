using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class SendMessageResponse : CommonResponse
    {
        [DataMember(Name = "result")]
        public MessageInfo Result { get; set; }
    }
}