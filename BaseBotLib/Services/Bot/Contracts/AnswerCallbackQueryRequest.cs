using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class AnswerCallbackQueryRequest
    {
        [DataMember(Name = "callback_query_id")]
        public string Id { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "show_alert")]
        public bool ShowAlert { get; set; }

        [DataMember(Name = "cache_time")]
        public int CacheTime { get; set; }
    }
}