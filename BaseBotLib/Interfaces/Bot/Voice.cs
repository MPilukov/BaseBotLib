namespace BaseBotLib.Interfaces.Bot
{
    public class Voice : File
    {
        public int Duration { get; set; }
        
        public string MimeType { get; set; }
    }
}