using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class GetFileResponse : CommonResponse
    {
        [DataMember(Name = "result")]
        public GetFileData Result { get; set; }
    }
}