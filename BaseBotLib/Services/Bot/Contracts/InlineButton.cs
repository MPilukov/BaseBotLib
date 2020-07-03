using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class InlineButton
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "callback_data")]
        public string CallbackData { get; set; }
    }
}
