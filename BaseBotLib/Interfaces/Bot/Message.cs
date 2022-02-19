namespace BaseBotLib.Interfaces.Bot
{
    public class Message
    {
        public string Text { get; set; }
        public string RequestText { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ChatId { get; set; }
        public string FileId { get; set; }

        public Document Document { get; set; }

        public Photo[] Photos { get; set; }

        public Voice Voice { get; set; }
    }
}