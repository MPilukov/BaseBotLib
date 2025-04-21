using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class SendAnyDocumentDataResponse
    {
        [DataMember(Name = "from")]
        public UserInfo UserData { get; set; }
        
        [DataMember(Name = "chat")]
        public ChatInfo ChatData { get; set; }

        [DataMember(Name = "photo")]
        public PhotoInfo[] Photos { get; set; }

        [DataMember(Name = "document")]
        public DocumentInfo DocumentData { get; set; }
        
        [DataMember(Name = "voice")]
        public VoiceInfo Voice { get; set; }
    }
}