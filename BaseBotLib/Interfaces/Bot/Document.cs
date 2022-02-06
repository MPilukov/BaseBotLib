using System.Runtime.Serialization;

namespace BaseBotLib.Interfaces.Bot
{
    [DataContract]
    public class Document : File
    {
        public string FileName { get; set; }

        public string MimeType { get; set; }
    }
}