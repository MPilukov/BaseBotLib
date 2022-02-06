using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class VoiceInfo : FileInfo
    {
        [DataMember(Name="duration")]
        public int Duration { get; set; }
        
        [DataMember(Name="mime_type")]
        public string MimeType { get; set; }
    }
}