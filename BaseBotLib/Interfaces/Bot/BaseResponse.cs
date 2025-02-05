namespace BaseBotLib.Interfaces.Bot
{
    public class BaseResponse
    {
        public bool Success { get; set; } = true;
        public string ErrorText { get; set; } = null;
    }
}