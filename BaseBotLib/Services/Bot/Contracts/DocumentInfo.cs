using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class DocumentInfo : FileInfo
    {
        [DataMember(Name = "file_name")]
        public string FileName { get; set; }

        [DataMember(Name = "mime_type")]
        public string MimeType { get; set; }
    }
}