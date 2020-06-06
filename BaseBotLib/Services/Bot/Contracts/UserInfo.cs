using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class UserInfo
    {
        [DataMember(Name="username")]
        public string UserName { get; set; }
        
        [DataMember(Name="id")]
        public int Id { get; set; }
        
        [DataMember(Name="first_name")]
        public string FirstName { get; set; }
        
        [DataMember(Name="last_name")]
        public string LastName { get; set; }
        
        [DataMember(Name="is_bot")]
        public bool IsBot { get; set; }
        
        [DataMember(Name="language_code")]
        public string LanguageCode { get; set; }
    }
}