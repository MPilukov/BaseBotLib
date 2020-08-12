namespace BaseBotLib.Interfaces.Bot
{
    public class Message
    {
        public string Text { get; set; }
        public string RequestText { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ChatId { get; set; }
    }
}