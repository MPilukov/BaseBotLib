using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class ResultInfo
    {
        [DataMember(Name = "message")]
        public MessageInfo Info { get; set; }

        [DataMember(Name = "update_id")]
        public int UpdateId { get; set; }

        [DataMember(Name = "callback_query")]
        public CallbackQuery CallbackQuery { get; set; }
    }
}