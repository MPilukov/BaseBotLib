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
        private string FileUrl { get; }
        private ILogger Logger { get; }
        private IStorage Storage { get; }

        private const string LastMessageIdxName = "LastMessageIdx";
        private const string LimitSelectName = "LimitSelect";
        private long LastMessageIdx { get; set; }
        private int LimitSelect { get; set; }

        public Bot(string id, string token, IStorage storage, ILogger logger = null)
        {
            Id = id;
            Url = $"https://api.telegram.org/bot{Id}:{token}";
            FileUrl = $"https://api.telegram.org/file/bot{Id}:{token}";
            Logger = logger;
            Storage = storage;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Init();
        }

        public Bot(string id, string token)
        {
            Id = id;
            Url = $"https://api.telegram.org/bot{Id}:{token}";
            FileUrl = $"https://api.telegram.org/file/bot{Id}:{token}";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Init();
        }

        private void Init()
        {
            LimitSelect = Storage == null ? 20 : Convert.ToInt32(Storage.GetValue(LimitSelectName).Result ?? "20");
            LastMessageIdx = Storage == null ? 0 : Convert.ToInt64(Storage.GetValue(LastMessageIdxName).Result ?? "0");

            var botName = GetBotName().Result;

            if (!string.IsNullOrWhiteSpace(botName))
            {
                Logger?.Info($"Bot launched - {botName}.");
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

            Logger?.Warn($"When receiving information about the bot ({Id}) we received an error : {response.ErrorDescription}.");
            return null;
        }

        public async Task<Message[]> GetNewMessages()
        {
            var url = $"{Url}/getUpdates?limit={LimitSelect}&offset={LastMessageIdx}";

            var result = Array.Empty<Message>();

            try
            {
                var response = await GetInternal<GetMessagesResponse>(url);

                if (response.IsSuccess)
                {
                    result = response.Result.Select(ConvertToMessage).Where(m => m != null).ToArray();

                    if (response.Result.Any())
                    {
                        LastMessageIdx = response.Result.Max(x => x.UpdateId) + 1;
                        if (Storage != null)
                        {
                            await Storage.SetValue(LastMessageIdxName, LastMessageIdx.ToString());
                        }
                    }
                }
                else
                {
                    Logger?.Warn($"When receiving a list of messages, the server returned an error : {response.ErrorDescription}.");
                }
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error getting list of messages : {exp}.");
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
                Logger?.Warn($"Error sending message to client : {text} / {chatId} : {exp}.");
            }
        }

        public Task CreateKeyboard(string chatId, string text, string[] texts)
        {
            return CreateKeyboardInternal(chatId, text, texts, true, true);
        }

        public Task CreateKeyboard(string chatId, string text, string[] texts, bool oneTime)
        {
            return CreateKeyboardInternal(chatId, text, texts, oneTime, true);
        }

        public Task CreateKeyboard(string chatId, string text, string[] texts, bool oneTime, bool resizeKeyboard)
        {
            return CreateKeyboardInternal(chatId, text, texts, oneTime, resizeKeyboard);
        }

        private Task CreateKeyboardInternal(string chatId, string text, string[] texts, bool oneTime, bool resizeKeyboard)
        {
            if (texts.Length == 0)
            {
                return Task.FromResult(0);
            }

            var data = GetButtons(texts);

            var body = new CreateKeyboardRequest
            {
                OneTime = oneTime,
                ResizeKeyboard = resizeKeyboard,
                Buttons = data,
            };

            var bodyString = JsonConvert.SerializeObject(body);

            var url = $"{Url}/sendMessage?chat_id={chatId}&text={HttpUtility.UrlEncode(text)}" +
                $"&reply_markup={HttpUtility.UrlEncode(bodyString)}";

            return PostInternal(url, new Dictionary<string, string>());
        }

        private static KeyboardButton[][] GetButtons(IReadOnlyList<string> texts)
        {
            var response = new List<KeyboardButton[]>();

            var count = 0;
            while (count < texts.Count)
            {
                if (texts.Count - count == 1)
                {
                    response.Add(new[] {
                        new KeyboardButton
                        {
                            Text = texts[count],
                        }
                    });

                    count += 1;
                }
                else
                {
                    response.Add(new[] {
                        new KeyboardButton
                        {
                            Text = texts[count],
                        },
                        new KeyboardButton
                        {
                            Text = texts[count + 1],
                        },
                    });

                    count += 2;
                }
            }

            return response.ToArray();
        }

        private static InlineButton[][] GetInlineButtons(IReadOnlyList<string> texts)
        {
            var response = new List<InlineButton[]>();

            var count = 0;
            while (count < texts.Count)
            {
                if (texts.Count - count == 1)
                {
                    response.Add(new[] {
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
                    response.Add(new[] {
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
                }
            }

            return response.ToArray();
        }

        public Task CreateInlineKeyboard(string chatId, string text, string[] texts)
        {
            return CreateInlineKeyboardInternal(chatId, text, texts, true, true);
        }

        public Task CreateInlineKeyboard(string chatId, string text, string[] texts, bool oneTime)
        {
            return CreateInlineKeyboardInternal(chatId, text, texts, oneTime, true);
        }

        public Task CreateInlineKeyboard(string chatId, string text, string[] texts, bool oneTime, bool resizeKeyboard)
        {
            return CreateInlineKeyboardInternal(chatId, text, texts, oneTime, resizeKeyboard);
        }

        public async Task<BaseResponse> SetWebhook(string webhookUrl, string secretToken)
        {
            try
            {
                var url = $"{Url}/setWebhook?url={webhookUrl}&secret_token={HttpUtility.UrlEncode(secretToken)}";
                await PostInternal(url, new Dictionary<string, string>());
                return new BaseResponse();
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error adding webhook {webhookUrl} : {exp}.");
                return new BaseResponse
                {
                    ErrorText = exp.Message,
                };
            }
        }

        public async Task<BaseResponse> DeleteWebhook()
        {
            try
            {
                var url = $"{Url}/deleteWebhook";
                await PostInternal(url, new Dictionary<string, string>());
                return new BaseResponse();
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error deleting webhook : {exp}.");
                return new BaseResponse
                {
                    ErrorText = exp.Message,
                };
            }
        }

        private Task CreateInlineKeyboardInternal(string chatId, string text, IReadOnlyList<string> texts, bool oneTime, bool resizeKeyboard)
        {
            if (texts.Count == 0)
            {
                return Task.FromResult(0);
            }

            var data = GetInlineButtons(texts);

            var body = new CreateInlineKeyboardRequest
            {
                OneTime = oneTime,
                ResizeKeyboard = resizeKeyboard,
                InlineButtons = data,
            };

            var bodyString = JsonConvert.SerializeObject(body);

            var url = $"{Url}/sendMessage?chat_id={chatId}&text={HttpUtility.UrlEncode(text)}" +
                $"&reply_markup={HttpUtility.UrlEncode(bodyString)}";

            return PostInternal(url, new Dictionary<string, string>());
        }

        private static async Task PostInternal(string url, Dictionary<string, string> headers)
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
                        $"Error executing request. {response.StatusCode} {await response.Content.ReadAsStringAsync()}.");
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
                        $"Error executing request. {response.StatusCode} {await response.Content.ReadAsStringAsync()}.");
                }
            }
        }
        private static async Task<T> GetInternal<T>(string url, Dictionary<string, string> headers = null)
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
                        $"Error executing request. {response.StatusCode} {await response.Content.ReadAsStringAsync()}.");
                }

                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
        }
        private Message ConvertToMessage(ResultInfo messageData)
        {
            if (messageData == null)
            {
                Logger?.Warn("Message is empty.");
                return null;
            }

            if (messageData.Info == null && messageData.CallbackQuery == null)
            {
                Logger?.Warn("Message information is empty.");
                return null;
            }

            if (messageData.Info?.UserData == null && messageData.CallbackQuery?.UserData == null)
            {
                Logger?.Warn("User information is empty.");
                return null;
            }

            if (messageData.Info?.ChatData == null && messageData.CallbackQuery?.Info?.ChatData == null)
            {
                Logger?.Warn("Chat information is empty.");
                return null;
            }

            return new Message
            {
                Id = messageData.Info?.MessageId ?? messageData.CallbackQuery?.Info?.MessageId ?? "-1",
                Text = messageData.Info?.Text ?? messageData.CallbackQuery?.Data,
                RequestText = messageData.CallbackQuery?.Info?.Text,
                FirstName = messageData.Info?.UserData?.FirstName ?? messageData.CallbackQuery?.UserData?.FirstName,
                LastName = messageData.Info?.UserData?.LastName ?? messageData.CallbackQuery?.UserData?.LastName,
                UserId = messageData.Info?.UserData?.Id ?? messageData.CallbackQuery?.UserData?.Id ?? "-1",
                UserName = messageData.Info?.UserData?.UserName ?? messageData.CallbackQuery?.UserData?.UserName,
                ChatId = messageData.Info?.ChatData?.Id ?? messageData.CallbackQuery?.Info?.ChatData?.Id ?? "-1",

                FileId = messageData.Info?.DocumentData?.FileId ??
                         messageData.Info?.Photos?.FirstOrDefault()?.FileId ??
                         messageData.Info?.Voice?.FileId,
                Document = messageData.Info?.DocumentData != null
                    ? new Document
                    {
                        FileId = messageData.Info.DocumentData.FileId,
                        FileUniqueId = messageData.Info.DocumentData.FileUniqueId,
                        FileName = messageData.Info.DocumentData.FileName,
                        FileSize = messageData.Info.DocumentData.FileSize,
                        MimeType = messageData.Info.DocumentData.MimeType,
                    }
                    : null,
                Voice = messageData.Info?.Voice != null
                    ? new Voice
                    {
                        FileId = messageData.Info.Voice.FileId,
                        FileUniqueId = messageData.Info.Voice.FileUniqueId,
                        FileSize = messageData.Info.Voice.FileSize,
                        MimeType = messageData.Info.Voice.MimeType,
                        Duration = messageData.Info.Voice.Duration,
                    }
                    : null,
                Photos = (messageData.Info?.Photos?.Any() ?? false)
                    ? messageData.Info?.Photos.Select(photo => new Photo
                    {
                        FileId = photo.FileId,
                        FileUniqueId = photo.FileUniqueId,
                        FileSize = photo.FileSize,
                        Width = photo.Width,
                        Height = photo.Height,
                    }).ToArray()
                    : Array.Empty<Photo>(),
            };
        }

        public async Task<byte[]> GetFile(string fileId)
        {
            var getFileByIdUrl = $"{Url}/getFile?file_id={fileId}";
            var getFileResponse = await GetInternal<GetFileResponse>(getFileByIdUrl);

            if (getFileResponse.IsSuccess)
            {
                var filePath = getFileResponse.Result.FilePath;

                var getFileUrl = $"{FileUrl}/{filePath}";

                try
                {
                    using (var client = new WebClient())
                    {
                        byte[] bytes = client.DownloadData(new Uri(getFileUrl));
                        return bytes;
                    }
                }
                catch (Exception exc)
                {
                    Logger?.Warn($"When receiving the file ({filePath}) " +
                       $"got an error : {exc.Message}.");
                    return null;
                }
            }

            Logger?.Warn($"When receiving file information ({fileId}) " +
                $"got an error : {getFileResponse.ErrorDescription}.");
            return null;
        }
    }
}