using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class SendAnyDocumentResponse : CommonResponse
    {
        [DataMember(Name = "result")]
        public SendAnyDocumentDataResponse Result { get; set; }
    }
}