using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class DocumentInfo
    {

        [DataMember(Name = "file_id")]
        public string FileId { get; set; }

        [DataMember(Name = "mime_type")]
        public string MimeType { get; set; }
    }
}