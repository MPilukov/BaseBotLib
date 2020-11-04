using BaseBotLib.Interfaces.Logger;
using BaseBotLib.Interfaces.Storage;
using BaseBotLib.Services.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BaseBotLib.Services.Storage
{
    public class FileStorage : IStorage
    {
        private readonly string _fileName;
        private readonly ILogger _logger;
        private readonly char _separator = ';';
        private readonly string _keyValueSeparator = "==";
        private Dictionary<string, string> Cash { get; set; }

        public FileStorage(string fileName, ILogger logger = null)
        {
            _fileName = fileName;
            _logger = logger ?? new ConsoleLogger();
            Cash = null;
        }

        private Dictionary<string, string> GetData()
        {
            if (Cash != null)
            {
                return Cash;
            }

            var fileData = ReadFile();

            if (string.IsNullOrWhiteSpace(fileData))
            {
                return new Dictionary<string, string>();
            }

            var dataArray = fileData.Split(_separator);

            var result = new Dictionary<string, string>();

            foreach (var item in dataArray)
            {
                var itemData = item ?? "";
                var pair = itemData.Split(new string[] { _keyValueSeparator }, StringSplitOptions.None);

                if (pair.Length != 2)
                {
                    continue;
                }

                if (!result.ContainsKey(pair[0]))
                {
                    result.Add(pair[0], pair[1]);
                }
            }

            return result;
        }

        private void SetData(Dictionary<string, string> pairs)
        {
            var dataArray = pairs.Select(x => $"{x.Key}{_keyValueSeparator}{x.Value}").ToArray();

            var dataString = string.Join(_separator.ToString(), dataArray);

            WriteToFile(dataString);
        }

        private bool ExistsOrCreate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_fileName))
                {
                    _logger?.Warn("Имя файла для сервис-хранилища пусто.");
                    return false;
                }

                var dir = Directory.GetParent(_fileName).FullName;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(_fileName))
                {
                    using (var file = File.Create(_fileName))
                    {

                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _logger?.Warn($"Ошибка при создании/проверки существования файла ({_fileName}) : {e}.");
                return false;
            }
        }

        private string ReadFile()
        {
            try
            {
                if (ExistsOrCreate())
                {
                    return File.ReadAllText(_fileName);
                }

                _logger.Warn("Ошибка при чтении данных из файла.");
                return null;
            }
            catch (Exception e)
            {
                _logger.Warn($"Ошибка при чтении данных из файла : {e}.");
                return null;
            }
        }

        private bool WriteToFile(string data)
        {
            try
            {
                if (ExistsOrCreate())
                {
                    File.WriteAllText(_fileName, data);
                    return true;
                }

                _logger.Warn("Ошибка при записи данных в файл.");
                return false;
            }
            catch (Exception e)
            {
                _logger.Warn($"Ошибка при записи данных в файл : {e}.");
                return false;
            }
        }

        public Task SetValue(string key, string value)
        {
            var data = GetData();

            if (data.ContainsKey(key))
            {
                data[key] = value;
            }
            else
            {
                data.Add(key, value);
            }

            SetData(data);

            UpdateCash(data);

            return Task.FromResult(0);
        }

        private void UpdateCash(Dictionary<string, string> data)
        {
            Cash = data;
        }

        public Task<string> GetValue(string key)
        {
            var data = GetData();

            data.TryGetValue(key, out var value);

            return Task.FromResult(value);
        }
    }
}