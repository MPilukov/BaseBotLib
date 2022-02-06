using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class GetMessagesResponse : CommonResponse
    {
        [DataMember(Name = "result")]
        public ResultInfo[] Result { get; set; }
    }
}