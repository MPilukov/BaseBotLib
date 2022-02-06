using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class FileInfo
    {
        [DataMember(Name="file_id")]
        public string FileId { get; set; }
        
        [DataMember(Name="file_unique_id")]
        public string FileUniqueId { get; set; }
        
        [DataMember(Name="file_size")]
        public int FileSize { get; set; }
    }
}