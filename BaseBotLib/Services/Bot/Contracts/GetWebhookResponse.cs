using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class GetWebhookResponse : CommonResponse
    {
        [DataMember(Name = "result")]
        public GetWebhookDataResponse Result { get; set; }
    }
}