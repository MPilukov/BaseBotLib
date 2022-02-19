using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class ChatInfo
    {
        [DataMember(Name="username")]
        public string UserName { get; set; }
        
        [DataMember(Name="id")]
        public string Id { get; set; }
        
        [DataMember(Name="first_name")]
        public string FirstName { get; set; }
        
        [DataMember(Name="last_name")]
        public string LastName { get; set; }
        
        [DataMember(Name="type")]
        public string Type { get; set; } // private
    }
}