using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class SetWebhookRequest : BaseBodyRequest
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }
        [DataMember(Name = "secret_token", EmitDefaultValue = false)]
        public string SecretToken { get; set; }
    }
}