using BaseBotLib.Interfaces.Logger;
using System;
using System.IO;

namespace BaseBotLib.Services.Logger
{
    public class FileConsoleLogger : ILogger
    {
        private readonly string _fileFolder;
        private readonly ILogger _defaultlogger;

        public FileConsoleLogger(string fileFolder, ILogger defaultlogger = null)
        {
            _fileFolder = fileFolder;
            _defaultlogger = defaultlogger ?? new ConsoleLogger();
        }

        private string CurrentTime()
        {
            return DateTime.UtcNow.ToString("HH:mm");
        }

        private void Write(string data)
        {
            try
            {
                if (!Directory.Exists(_fileFolder))
                {
                    Directory.CreateDirectory(_fileFolder);
                }

                var fileName = $"{_fileFolder}/logs_{DateTime.UtcNow.Date:dd.MM.yyyy}.txt";

                File.AppendAllText(fileName, data + Environment.NewLine);
            }
            catch (Exception e)
            {
                _defaultlogger.Warn($"Ошибка при записи логов в файл : {e}.");
            }
        }

        public void Info(string text)
        {
            var content = $"Info ({CurrentTime()}) : {text}";

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(content);
            Write(content);
        }

        public void Warn(string text)
        {
            var content = $"Warn ({CurrentTime()}) : {text}";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(content);

            Write(content);
        }

        public void Error(string text)
        {
            var content = $"Error ({CurrentTime()}) : {text}";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(content);

            Write(content);
        }
    }
}