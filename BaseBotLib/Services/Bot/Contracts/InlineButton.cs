using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class InlineButton
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "callback_data", EmitDefaultValue = false)]
        public string CallbackData { get; set; }
        
        [DataMember(Name = "url", EmitDefaultValue = false)]
        public string Url { get; set; }
    }
}