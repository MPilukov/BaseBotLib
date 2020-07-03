using System.Runtime.Serialization;

namespace BaseBotLib.Services.Bot.Contracts
{
    [DataContract]
    public class CreateKeyboardRequest
    {
        [DataMember(Name = "keyboard")]
        public KeyboardButton[][] Buttons { get; set; }

        [DataMember(Name = "one_time_keyboard")]
        public bool OneTime { get; set; }

        [DataMember(Name = "resize_keyboard")]
        public bool ResizeKeyboard { get; set; }
    }
}
