using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class MessageInfo
    {
        [DataMember(Name = "date")]
        public string Date { get; set; }
        
        [DataMember(Name = "text")]
        public string Text { get; set; }
        
        [DataMember(Name = "message_id")]
        public string MessageId { get; set; }
        
        [DataMember(Name = "from")]
        public UserInfo UserData { get; set; }
        
        [DataMember(Name = "chat")]
        public ChatInfo ChatData { get; set; }

        [DataMember(Name = "document")]
        public DocumentInfo DocumentData { get; set; }

        [DataMember(Name = "photo")]
        public PhotoInfo[] Photos { get; set; }

        [DataMember(Name = "voice")]
        public VoiceInfo Voice { get; set; }
    }
}