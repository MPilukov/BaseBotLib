using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class GetMeResponse : CommonResponse
    {
        [DataMember(Name = "result")]
        public UserInfo BotInfo { get; set; }
    }
}