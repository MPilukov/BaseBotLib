﻿using BaseBotLib.Interfaces.Bot;
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
using BaseBotLib.Interfaces.Bot.Menu;

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

        public async Task<string> GetBotUserName()
        {
            var url = $"{Url}/getMe";

            var response = await GetInternal<GetMeResponse>(url);

            if (response.IsSuccess)
            {
                return response.BotInfo?.UserName;
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

        public async Task<SendMessageBaseResponse> SendMessage(string chatId, string text)
        {
            return await SendMessageInternal(chatId, text);
        }

        public async Task<BaseResponse> EditMessage(string chatId, string messageId, string text)
        {
            return await EditMessageInternal(chatId, messageId, text);
        }

        public async Task<BaseResponse> DeleteMessage(string chatId, string messageId)
        {
            return await DeleteMessageInternal(chatId, messageId);
        }

        public Task<BaseResponse> SendSelectionMenu(string chatId, SelectionMenu menu)
        {
            return SendSelectionMenuInternal(chatId, menu);
        }

        public Task<SendMessageBaseResponse> SendInlineSelectionMenu(string chatId, InlineSelectionMenu menu)
        {
            return SendInlineSelectionMenuInternal(chatId, menu);
        }

        public Task<BaseResponse> EditInlineSelectionMenu(string chatId, string messageId, InlineSelectionMenu menu)
        {
            return EditExistInlineSelectionMenuInternal(chatId, messageId, menu);
        }

        public Task<BaseResponse> DeleteExistInlineSelectionMenu(string chatId, string messageId)
        {
            return EditExistInlineSelectionMenuInternal(chatId, messageId, keyboardRequest: null);
        }

        public async Task<SendMessageBaseResponse> SendMessageWithMarkdown(string chatId, string text)
        {
            return await SendMessageInternal(chatId, text, ParseModeMarkdownV2);
        }
        public async Task<SendMessageBaseResponse> SendMessageWithHtml(string chatId, string text)
        {
            return await SendMessageInternal(chatId, text, ParseModeHTML);
        }
        
        /// <summary>
        /// 10 MB max size for photos, 50 MB for other files.
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="fileName"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<SendFileBaseResponse> SendPhoto(string chatId, string fileName, byte[] body)
        {
            var response = await SendAnyDocumentInternal(chatId, "photo", fileName, body);
            var fileId = response?.Result?.Photos?.FirstOrDefault()?.FileId;

            return new SendFileBaseResponse
            {
                FileId = fileId,
                ErrorText = response?.ErrorDescription,
                Success = response?.IsSuccess ?? false,
            };
        }
        
        /// <summary>
        /// 10 MB max size for photos, 50 MB for other files.
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="fileName"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<SendFileBaseResponse> SendFile(string chatId, string fileName, byte[] body)
        {
            var response = await SendAnyDocumentInternal(chatId, "document", fileName, body);
            var fileId = response?.Result?.DocumentData?.FileId;

            return new SendFileBaseResponse
            {
                FileId = fileId,
                ErrorText = response?.ErrorDescription,
                Success = response?.IsSuccess ?? false,
            };
        }
        
        private async Task<SendAnyDocumentResponse> SendAnyDocumentInternal(
            string chatId, string docType, string fileName, byte[] body)
        {
            try
            {
                var url = string.Equals("photo", docType, StringComparison.InvariantCultureIgnoreCase)
                    ? $"{Url}/sendPhoto?chat_id={chatId}"
                    : $"{Url}/sendDocument?chat_id={chatId}";
                
                var response = await PostInternalMultiFormData<SendAnyDocumentResponse>(url, docType, fileName, body);
                return response;
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error sending file to client : {chatId} : {exp}.");
                return new SendAnyDocumentResponse
                {
                    IsSuccess = false,
                    ErrorDescription = exp.Message,
                };
            }
        }
        
        private async Task<BaseResponse> DeleteMessageInternal(string chatId, string messageId)
        {
            try
            {
                var url = $"{Url}/deleteMessage?chat_id={chatId}&message_id={messageId}";
                await PostInternal(url, new Dictionary<string, string>());
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error deleting message : {exp}.");
                return new BaseResponse
                {
                    ErrorText = exp.Message,
                };
            }

            return new BaseResponse();
        }
        
        private async Task<BaseResponse> EditMessageInternal(string chatId, string messageId, string text)
        {
            try
            {
                var url = $"{Url}/editMessageText";

                var body = new EditMessageTextRequest
                {
                    ChatId = chatId,
                    ExistMessageId = messageId,
                    Text = text,
                };
                await PostInternal(url, body, new Dictionary<string, string>());
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error editing message : {exp}.");
                return new BaseResponse
                {
                    ErrorText = exp.Message,
                };
            }

            return new BaseResponse();
        }
    
        private async Task<SendMessageBaseResponse> SendMessageInternal(string chatId, string text, string parseMode = null)
        {
            try
            {
                var url = $"{Url}/sendMessage";

                var body = string.IsNullOrWhiteSpace(parseMode)
                    ? new SendMessageRequest
                    {
                        ChatId = chatId,
                        Text = text,
                    }
                    : new ExtendedSendMessageRequest
                    {
                        ChatId = chatId,
                        Text = text,
                        ParseMode = parseMode,
                    };
                var response = await PostInternalWithResponse<SendMessageRequest, SendMessageResponse>(
                    url, body, new Dictionary<string, string>());
                
                return new SendMessageBaseResponse
                {
                    Success = response.Result != null,
                    ErrorText = response?.ErrorDescription,
                    MessageId = response?.Result?.MessageId,
                };
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error sending message : {exp}.");
                return new SendMessageBaseResponse
                {
                    ErrorText = exp.Message,
                };
            }
        }
    
        private static string ParseModeMarkdownV2 = "MarkdownV2";
        private static string ParseModeHTML = "HTML";

        [Obsolete]
        public Task CreateKeyboard(string chatId, string text, string[] texts)
        {
            return CreateKeyboardInternal(chatId, text, texts, true, true);
        }

        [Obsolete]
        public Task CreateKeyboard(string chatId, string text, string[] texts, bool oneTime)
        {
            return CreateKeyboardInternal(chatId, text, texts, oneTime, true);
        }

        [Obsolete]
        public Task CreateKeyboard(string chatId, string text, string[] texts, bool oneTime, bool resizeKeyboard)
        {
            return CreateKeyboardInternal(chatId, text, texts, oneTime, resizeKeyboard);
        }

        [Obsolete]
        private Task CreateKeyboardInternal(string chatId, string text, string[] texts, bool oneTime, bool resizeKeyboard)
        {
            if (texts.Length == 0)
            {
                return Task.FromResult(0);
            }

            var data = GetButtons(texts.Select(x => new SelectionMenuButton
            {
                Text = x,
            }).ToArray());

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

        private async Task<BaseResponse> SendSelectionMenuInternal(string chatId, SelectionMenu menu)
        {
            var response = new BaseResponse();
            
            try
            {
                if (menu.Buttons.Count == 0)
                {
                    return response;
                }

                var buttons = GetButtons(menu.Buttons);
                
                var body = new SelectionMenuRequest
                {
                    ChatId = chatId,
                    Text = menu.MenuText,
                    CreateKeyboardRequest = new CreateKeyboardRequest
                    {
                        OneTime = menu.OneTime,
                        ResizeKeyboard = menu.ResizeKeyboard,
                        Buttons = buttons,
                    },
                };
                
                var url = $"{Url}/sendMessage";

                await PostInternal(url, body, new Dictionary<string, string>());
                
                return response;
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error sending selection menu : {exp}.");
                return new BaseResponse
                {
                    ErrorText = exp.Message,
                };
            }
        }

        private async Task<BaseResponse> EditExistInlineSelectionMenuInternal(
            string chatId, string messageId, InlineSelectionMenu menu)
        {
            try
            {
                if (menu.Buttons.Count == 0)
                {
                    return new BaseResponse
                    {
                        ErrorText = "Inline menu is empty.",
                    };
                }

                var buttons = GetInlineButtons(menu.Buttons, menu.ButtonsPerRow);
                
                var createKeyboardRequest = new CreateInlineKeyboardRequest
                {
                    OneTime = menu.OneTime,
                    ResizeKeyboard = menu.ResizeKeyboard,
                    InlineButtons = buttons,
                };

                return await EditExistInlineSelectionMenuInternal(chatId, messageId, createKeyboardRequest);
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error sending inline selection menu : {exp}.");
                return new BaseResponse
                {
                    ErrorText = exp.Message,
                };
            }
        }
        
        private async Task<BaseResponse> EditExistInlineSelectionMenuInternal(
            string chatId, string messageId, CreateInlineKeyboardRequest keyboardRequest)
        {
            var response = new BaseResponse();

            try
            {
                var body = keyboardRequest == null
                    ? new DeleteInlineMenuRequest
                    {
                        ChatId = chatId,
                        MessageId = messageId,
                    }
                    : new EditInlineMenuRequest
                    {
                        ChatId = chatId,
                        MessageId = messageId,
                        InlineKeyboardRequest = keyboardRequest,
                    };
                
                var url = $"{Url}/editMessageReplyMarkup";

                await PostInternal(url, body, new Dictionary<string, string>());

                return response;
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error sending inline selection menu : {exp}.");
                return new BaseResponse
                {
                    ErrorText = exp.Message,
                };
            }
        }

        private async Task<SendMessageBaseResponse> SendInlineSelectionMenuInternal(string chatId, InlineSelectionMenu menu)
        {
            var response = new SendMessageBaseResponse();

            try
            {
                if (menu.Buttons.Count == 0)
                {
                    return response;
                }

                var buttons = GetInlineButtons(menu.Buttons, menu.ButtonsPerRow);
                
                var body = new InlineSelectionMenuRequest
                {
                    ChatId = chatId,
                    Text = menu.MenuText,
                    CreateInlineKeyboardRequest = new CreateInlineKeyboardRequest
                    {
                        OneTime = menu.OneTime,
                        ResizeKeyboard = menu.ResizeKeyboard,
                        InlineButtons = buttons,
                    },
                };
                
                var url = $"{Url}/sendMessage";
                
                var sendMessageResponse = await PostInternalWithResponse<InlineSelectionMenuRequest, SendMessageResponse>(
                    url, body, new Dictionary<string, string>());
                // await PostInternal(url, body, new Dictionary<string, string>());

                return new SendMessageBaseResponse
                {
                    Success = sendMessageResponse.Result != null,
                    ErrorText = sendMessageResponse?.ErrorDescription,
                    MessageId = sendMessageResponse?.Result?.MessageId,
                };
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error sending inline selection menu : {exp}.");
                return new SendMessageBaseResponse
                {
                    ErrorText = exp.Message,
                };
            }
        }

        private static KeyboardButton[][] GetButtons(IReadOnlyList<SelectionMenuButton> buttons)
        {
            var response = new List<KeyboardButton[]>();

            var count = 0;
            while (count < buttons.Count)
            {
                if (buttons.Count - count == 1)
                {
                    response.Add(new[] {
                        new KeyboardButton
                        {
                            Text = buttons[count].Text,
                        }
                    });

                    count += 1;
                }
                else
                {
                    response.Add(new[] {
                        new KeyboardButton
                        {
                            Text = buttons[count].Text,
                        },
                        new KeyboardButton
                        {
                            Text = buttons[count + 1].Text,
                        },
                    });

                    count += 2;
                }
            }

            return response.ToArray();
        }

        private static InlineButton[][] GetInlineButtonsWithTwoColumn(IReadOnlyList<InlineSelectionMenuButton> buttons)
        {
            var response = new List<InlineButton[]>();

            var count = 0;
            while (count < buttons.Count)
            {
                if (buttons.Count - count == 1)
                {
                    response.Add(new[] {
                        new InlineButton
                        {
                            Text = buttons[count].Text,
                            CallbackData = buttons[count].CallbackData,
                            Url = buttons[count].Url,
                        }
                    });

                    count += 1;
                }
                else
                {
                    response.Add(new[] {
                        new InlineButton
                        {
                            Text = buttons[count].Text,
                            CallbackData = buttons[count].CallbackData,
                            Url = buttons[count].Url,
                        },
                        new InlineButton
                        {
                            Text = buttons[count + 1].Text,
                            CallbackData = buttons[count + 1].CallbackData,
                            Url = buttons[count + 1].Url,
                        },
                    });

                    count += 2;
                }
            }

            return response.ToArray();
        }

        private static InlineButton[][] GetInlineButtons(IReadOnlyList<InlineSelectionMenuButton> buttons, int buttonsPerRow)
        {
            var response = new List<InlineButton[]>();

            var count = 0;
            while (count < buttons.Count)
            {
                // Вычисляем, сколько кнопок можно добавить в текущую строку
                var remaining = buttons.Count - count;
                var currentRowCount = Math.Min(buttonsPerRow, remaining);

                var row = new InlineButton[currentRowCount];

                for (var i = 0; i < currentRowCount; i++)
                {
                    row[i] = new InlineButton
                    {
                        Text = buttons[count + i].Text,
                        CallbackData = buttons[count + i].CallbackData,
                        Url = buttons[count + i].Url
                    };
                }

                response.Add(row);
                count += currentRowCount;
            }

            return response.ToArray();
        }

        [Obsolete]
        private static InlineButton[][] GetOldInlineButtons(IReadOnlyList<string> texts)
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

        [Obsolete]
        public Task CreateInlineKeyboard(string chatId, string text, string[] texts)
        {
            return CreateInlineKeyboardInternal(chatId, text, texts, true, true);
        }

        [Obsolete]
        public Task CreateInlineKeyboard(string chatId, string text, string[] texts, bool oneTime)
        {
            return CreateInlineKeyboardInternal(chatId, text, texts, oneTime, true);
        }

        [Obsolete]
        public Task CreateInlineKeyboard(string chatId, string text, string[] texts, bool oneTime, bool resizeKeyboard)
        {
            return CreateInlineKeyboardInternal(chatId, text, texts, oneTime, resizeKeyboard);
        }

        public async Task<BaseResponse> SetWebhook(string webhookUrl, string secretToken)
        {
            try
            {
                var body = new SetWebhookRequest
                {
                    Url = webhookUrl,
                    SecretToken = secretToken,
                };
                
                var url = $"{Url}/setWebhook";
                await PostInternal(url, body, new Dictionary<string, string>());
                
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
        public async Task<Webhook> GetWebhooks()
        {
            try
            {
                var url = $"{Url}/getWebhookInfo";
                var response = await GetInternal<GetWebhookResponse>(url);
                return new Webhook
                {
                    Url = response.Result.Url, 
                };
            }
            catch (Exception exp)
            {
                Logger?.Warn($"Error getting webhooks : {exp}.");
                return new Webhook
                {
                    ErrorText = exp.Message,
                };
            }
        }

        [Obsolete]
        private Task CreateInlineKeyboardInternal(string chatId, string text, IReadOnlyList<string> texts, bool oneTime, bool resizeKeyboard)
        {
            if (texts.Count == 0)
            {
                return Task.FromResult(0);
            }

            var data = GetOldInlineButtons(texts);

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
        
        private async Task<TRes> PostInternalWithResponse<TReq, TRes>(string url, TReq body, Dictionary<string, string> headers)
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
                
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TRes>(str);
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

                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(str);
            }
        }
        

        private static async Task<T> PostInternalMultiFormData<T>(string url, string type, string fileName, byte[] bytes)
        {
            using (var multipart = new MultipartFormDataContent())
            {
                using (var fileContent = new ByteArrayContent(bytes))
                {
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    multipart.Add(fileContent, name: type, fileName : fileName);
                
                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.PostAsync(url, multipart);
                        var responseString = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception(
                                $"Error executing request. {response.StatusCode} {responseString}.");
                        }

                        return JsonConvert.DeserializeObject<T>(responseString);
                    }
                }
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