using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class KeyboardButton
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }
    }
}
