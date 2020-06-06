namespace BaseBotLib.Interfaces.Logger
{
    public interface ILogger
    {
        void Info(string text);
        void Warn(string text);
        void Error(string text);
    }
}
