using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class PhotoInfo : FileInfo
    {
        [DataMember(Name="width")]
        public int Width { get; set; }
        
        [DataMember(Name="height")]
        public int Height { get; set; }
    }
}