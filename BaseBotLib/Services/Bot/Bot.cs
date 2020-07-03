using BaseBotLib.Interfaces.Bot;
using BaseBotLib.Interfaces.Logger;
using BaseBotLib.Interfaces.Storage;
using BaseBotLib.Services.Bot.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace BaseBotLib.Services.Bot
{
    public class Bot : IBot
    {
        private string Id { get; }
        private string Url { get; }
        private ILogger Logger { get; }
        private IStorage Storage { get; }

        private string _lastMessageIdxName = "LastMessageIdx";
        private string _limitSelectName = "LimitSelect";
        private int LastMessageIdx { get; set; }
        private int LimitSelect { get; set; }

        public Bot(string id, string token, IStorage storage, ILogger logger = null)
        {
            Id = id;
            Url = $"https://api.telegram.org/bot{Id}:{token}";
            Logger = logger;
            Storage = storage;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Init();
        }

        private void Init()
        {
            LimitSelect = Convert.ToInt32(Storage.GetValue(_limitSelectName).Result ?? "20");
            LastMessageIdx = Convert.ToInt32(Storage.GetValue(_lastMessageIdxName).Result ?? "0");

            var botName = GetBotName().Result;

            if (!string.IsNullOrWhiteSpace(botName))
            {
                Logger?.Info($"Запущен бот - {botName}.");
            }
        }

        public async Task<string> GetBotName()
        {
            var url = $"{Url}/getMe";

            var response = await GetInternal<GetMeResponse>(url);

            if (response.IsSuccess)
            {
                return response.BotInfo?.FirstName;
            }

            Logger?.Warn($"При получении информации о боте ({Id}) получили ошибку : {response.ErrorDescription}.");
            return null;
        }

        public async Task<Message[]> GetNewMessages()
        {
            var url = $"{Url}/getUpdates?limit={LimitSelect}&offset={LastMessageIdx}";

            var result = new Message[0];

            try
            {
                var response = await GetInternal<GetMessagesResponse>(url);

                if (response.IsSuccess)
                {
                    result = response.Result.Select(ConvertToMessage).Where(m => m != null).ToArray();

                    if (response.Result.Any())
                    {
                        LastMessageIdx = response.Result.Max(x => x.UpdateId) + 1;
                        Storage.SetValue(_lastMessageIdxName, LastMessageIdx.ToString()).Wait();
                    }
                }
                else
                {
                    Logger?.Warn($"При получении списка сообщений сервер вернул ошибку : {response.ErrorDescription}.");
                }
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Ошибка при получении списка сообщений : {exp.ToString()}.");
            }

            return result;
        }

        public async Task SendMessage(string chatId, string text)
        {
            try
            {
                var url = $"{Url}/sendMessage?chat_id={chatId}&text={HttpUtility.UrlEncode(text)}";
                await PostInternal(url, new Dictionary<string, string>());
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Ошибка при отправке сообщения клиенту : {text} / {chatId} : {exp.ToString()}.");
            }
        }
        public Task CreateKeyboard(string chatId, string text, string[] texts)
        {
            if (texts.Length == 0)
            {
                return Task.FromResult(0);
            }

            var data = new KeyboardButton[][]
            {
                texts.Select(x =>
                new KeyboardButton
                {
                    Text = x,
                }).ToArray(),
            };

            var body = new CreateKeyboardRequest
            {
                OneTime = true,
                Buttons = data,
            };

            var bodyString = JsonConvert.SerializeObject(body);

            var url = $"{Url}/sendMessage?chat_id={chatId}&text={HttpUtility.UrlEncode(text)}" +
                $"&reply_markup={HttpUtility.UrlEncode(bodyString)}";

            return PostInternal(url, new Dictionary<string, string>());
        }

        private InlineButton[][] GetButtons(string[] texts)
        {
            var response = new List<InlineButton[]>();

            var count = 0;
            while (count < texts.Length)
            {
                if (texts.Length - count == 1)
                {
                    response.Add(new InlineButton[] {
                        new InlineButton
                        {
                            Text = texts[count],
                            CallbackData = texts[count],
                        }
                    });

                    count += 1;
                }
                else
                {
                    response.Add(new InlineButton[] {
                        new InlineButton
                        {
                            Text = texts[count],
                            CallbackData = texts[count],
                        },
                        new InlineButton
                        {
                            Text = texts[count + 1],
                            CallbackData = texts[count + 1],
                        },
                    });

                    count += 2;
                };
            }

            return response.ToArray();
        }

        public Task CreateInlineKeyboard(string chatId, string text, string[] texts)
        {
            if (texts.Length == 0)
            {
                return Task.FromResult(0);
            }

            var data = GetButtons(texts);

            var body = new CreateInlineKeyboardRequest
            {
                OneTime = true,
                ResizeKeyboard = true,
                InlineButtons = data,
            };

            var bodyString = JsonConvert.SerializeObject(body);

            var url = $"{Url}/sendMessage?chat_id={chatId}&text={HttpUtility.UrlEncode(text)}" +
                $"&reply_markup={HttpUtility.UrlEncode(bodyString)}";

            return PostInternal(url, new Dictionary<string, string>());
        }

        private async Task PostInternal(string url, Dictionary<string, string> headers)
        {
            var content = new StringContent(string.Empty);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    content.Headers.Add(header.Key, header.Value);
                }
            }

            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Ошибка при выполнении запроса. {response.StatusCode} {await response.Content.ReadAsStringAsync()}.");
                }
            }
        }
        private async Task PostInternal<T>(string url, T body, Dictionary<string, string> headers)
        {
            var bodyString = JsonConvert.SerializeObject(body);
            var content = new StringContent(bodyString);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    content.Headers.Add(header.Key, header.Value);
                }
            }

            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Ошибка при выполнении запроса. {response.StatusCode} {await response.Content.ReadAsStringAsync()}.");
                }
            }
        }
        private async Task<T> GetInternal<T>(string url, Dictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Ошибка при выполнении запроса. {response.StatusCode} {await response.Content.ReadAsStringAsync()}.");
                }

                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
        }
        private Message ConvertToMessage(MessageData messageData)
        {
            if (messageData == null)
            {
                Logger.Warn("Cообщение пусто.");
                return null;
            }

            if (messageData.Info == null && messageData.CallbackQuery == null)
            {
                Logger.Warn("Информация о сообщении пуста.");
                return null;
            }

            if (messageData.Info?.UserData == null && messageData.CallbackQuery?.UserData == null)
            {
                Logger.Warn("Информация о пользователе пуста.");
                return null;
            }

            if (messageData.Info?.ChatData == null && messageData.CallbackQuery?.Info?.ChatData == null)
            {
                Logger.Warn("Информация о чате пуста.");
                return null;
            }

            return new Message
            {
                Id = messageData.Info?.MessageId ?? messageData.CallbackQuery.Info.MessageId,
                Text = messageData.Info?.Text ?? messageData.CallbackQuery.Data,
                RequestText = messageData.CallbackQuery?.Info?.Text,
                FirstName = messageData.Info?.UserData?.FirstName ?? messageData.CallbackQuery.UserData.FirstName,
                LastName = messageData.Info?.UserData?.LastName ?? messageData.CallbackQuery.UserData.LastName,
                UserId = messageData.Info?.UserData?.Id ?? messageData.CallbackQuery.UserData.Id,
                UserName = messageData.Info?.UserData.UserName ?? messageData.CallbackQuery.UserData.UserName,
                ChatId = messageData.Info?.ChatData?.Id ?? messageData.CallbackQuery.Info.ChatData.Id,
            };
        }
    }
}
