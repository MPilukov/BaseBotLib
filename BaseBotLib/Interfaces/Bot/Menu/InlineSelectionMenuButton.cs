namespace BaseBotLib.Interfaces.Bot.Menu
{
    public class InlineSelectionMenuButton
    {
        public string Text { get; set; }
        public string CallbackData { get; set; }
        public string Url { get; set; }
        
        // todo: login_url
        // web_app
        // copy_text
        // pay
    }
}