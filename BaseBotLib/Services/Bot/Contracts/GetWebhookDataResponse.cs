using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class GetWebhookDataResponse
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}