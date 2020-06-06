using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class CommonResponse
    {
        [DataMember(Name = "ok")]
        public bool IsSuccess { get; set; }

        [DataMember(Name = "description")]
        public string ErrorDescription { get; set; }
    }
}