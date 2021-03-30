using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class GetFileResponse : CommonResponse
    {
        [DataMember(Name = "result")]
        public FileData Result { get; set; }
    }


    [DataContract]
    public class FileData
    {
        [DataMember(Name = "file_id")]
        public string FileId { get; set; }

        [DataMember(Name = "file_path")]
        public string FilePath { get; set; }
    }
}