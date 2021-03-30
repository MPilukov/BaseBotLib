using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class GetMessagesResponse : CommonResponse
    {
        [DataMember(Name = "result")]
        public MessageData[] Result { get; set; }
    }

    [DataContract]
    public class MessageData
    {
        [DataMember(Name = "message")]
        public MessageInfo Info { get; set; }

        [DataMember(Name = "update_id")]
        public int UpdateId { get; set; }

        [DataMember(Name = "callback_query")]
        public CallbackQuery CallbackQuery { get; set; }
    }

    [DataContract]
    public class MessageInfo
    {
        [DataMember(Name = "date")]
        public string Date { get; set; }
        
        [DataMember(Name = "text")]
        public string Text { get; set; }
        
        [DataMember(Name = "message_id")]
        public int MessageId { get; set; }
        
        [DataMember(Name = "from")]
        public UserInfo UserData { get; set; }
        
        [DataMember(Name = "chat")]
        public ChatInfo ChatData { get; set; }

        [DataMember(Name = "document")]
        public DocumentInfo DocumentData { get; set; }

        [DataMember(Name = "photo")]
        public DocumentInfo[] Photos { get; set; }
    }

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