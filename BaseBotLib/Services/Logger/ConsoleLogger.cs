using BaseBotLib.Interfaces.Logger;
using System;

namespace BaseBotLib.Services.Logger
{
    public class ConsoleLogger : ILogger
    {
        private string CurrentTime()
        {
            return DateTime.UtcNow.ToString("HH:mm");
        }

        public void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Info ({CurrentTime()}) : {text}");
        }

        public void Warn(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warn ({CurrentTime()}): {text}");
        }

        public void Error(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error ({CurrentTime()}): {text}");
        }
    }
}