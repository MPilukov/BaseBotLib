namespace BaseBotLib.Interfaces.Bot.Menu
{
    public class BaseMenu
    {
        public string MenuText { get; set; }
        public bool OneTime { get; set; } = true;
        public bool ResizeKeyboard { get; set; } = true;
    }
}