using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class CallbackQuery
    {
        [DataMember(Name = "data")]
        public string Data { get; set; }

        [DataMember(Name = "from")]
        public UserInfo UserData { get; set; }

        [DataMember(Name = "message")]
        public MessageInfo Info { get; set; }
    }
}