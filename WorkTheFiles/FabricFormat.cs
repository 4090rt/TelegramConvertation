using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramConvertorBots.HandleDOcumentAsync;
using TelegramConvertorBots.Models;
using TelegramConvertorBots.SendEmail;

namespace TelegramConvertorBots.WorkTheFiles
{
        public interface Formats
        {
             Task CurrentFormats(string formalower, long ChatId, string Filepath, CancellationToken cancellation);        
        }

        public class WORDtoPDF : Formats
        {
            public readonly ITelegramBotClient _botclient;
            public readonly Dictionary<long, Models.UserSession> _userSession;
            public readonly Microsoft.Extensions.Logging.ILogger _logger;

            public WORDtoPDF(ITelegramBotClient botClient,
                    Dictionary<long, Models.UserSession> userSession,
                     Microsoft.Extensions.Logging.ILogger logger)
            {
                _botclient = botClient;
                _userSession = userSession;
                _logger = logger;
            }

            public async Task CurrentFormats(string formalower, long ChatId, string Filepath, CancellationToken cancellation)
            {
                try
                {
                    if (formalower == "doc" || formalower == "docx")
                    {
                        var session = _userSession[ChatId];
                        WORDToPDF wordpdf = new WORDToPDF();
                        var converteredfileword_pdf = await wordpdf.DOCXConverttoPDF(Filepath);
                        if (converteredfileword_pdf == "Ошибка конертации" || converteredfileword_pdf == "Файл слишком большой для конвертации")
                        {
                            await _botclient.SendTextMessageAsync(
                                chatId: ChatId,
                                text: $"❌ {converteredfileword_pdf}",
                                cancellationToken: cancellation
                            );
                            return;
                        }
                        if (session.Email != null)
                        {
                            SendEmail.Send senn = new SendEmail.Send();
                            await senn.SmptServerSend(session.Email, converteredfileword_pdf);
                        }
                        await Task.Delay(300);

                        SendDocument senddocumentpdf_word = new SendDocument(_botclient, _logger, _userSession);
                        await senddocumentpdf_word.SendDocumentToChatAsync(ChatId, converteredfileword_pdf, cancellation);

                        DocumentDowloaded doc = new DocumentDowloaded(_botclient, _userSession, _logger);
                        doc.TryDeleteFile(converteredfileword_pdf);

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Не удалось получить конвертировать из pdf в word" + ex.Message);
                }
            }
        }


        public class PDFtoWord : Formats
        {
            public readonly ITelegramBotClient _botclient;
            public readonly Dictionary<long, Models.UserSession> _userSession;
            public readonly Microsoft.Extensions.Logging.ILogger _logger;

            public PDFtoWord(ITelegramBotClient botClient,
                   Dictionary<long, Models.UserSession> userSession,
                    Microsoft.Extensions.Logging.ILogger logger)
            {
                _botclient = botClient;
                _userSession = userSession;
                _logger = logger;
            }
            public async Task CurrentFormats(string formalower, long ChatId, string Filepath, CancellationToken cancellation)
            {
                try
                {
                    if (formalower == "pdf")
                    {
                        var session = _userSession[ChatId];
                        PDFToWord pDFToWord = new PDFToWord();
                        var converteredfilePDF_WORD = await pDFToWord.PDFConverttoDocx(Filepath);
                        if (converteredfilePDF_WORD == "Ошибка конертации" || converteredfilePDF_WORD == "Файл слишком большой для конвертации")
                        {
                            await _botclient.SendTextMessageAsync(
                                chatId: ChatId,
                                text: $"❌ {converteredfilePDF_WORD}",
                                cancellationToken: cancellation
                            );
                            return;
                        }
                        if (session.Email != null)
                        {
                            SendEmail.Send senn = new SendEmail.Send();
                            await senn.SmptServerSend(session.Email, converteredfilePDF_WORD);
                        }
                        await Task.Delay(300);

                        SendDocument senddocumentpdf_word = new SendDocument(_botclient, _logger, _userSession);
                        await senddocumentpdf_word.SendDocumentToChatAsync(ChatId, converteredfilePDF_WORD, cancellation);

                        DocumentDowloaded doc = new DocumentDowloaded(_botclient, _userSession, _logger);
                        doc.TryDeleteFile(converteredfilePDF_WORD);

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Не удалось получить конвертировать из pdf в word" + ex.Message);
                }
            }
        }

        public class TEXTtoWord : Formats
        {
            public readonly ITelegramBotClient _botclient;
            public readonly Dictionary<long, Models.UserSession> _userSession;
            public readonly Microsoft.Extensions.Logging.ILogger _logger;
            public TEXTtoWord(ITelegramBotClient botClient,
                   Dictionary<long, Models.UserSession> userSession,
                    Microsoft.Extensions.Logging.ILogger logger)
            {
                _botclient = botClient;
                _userSession = userSession;
                _logger = logger;
            }
            public async Task CurrentFormats(string formalower, long ChatId, string Filepath, CancellationToken cancellation)
            {
                try
                {
                    if (formalower == "txt")
                    {
                        var session = _userSession[ChatId];
                        TXTToWord tXTToWord = new TXTToWord();
                        var converteredTXT_Word = await tXTToWord.DOCXConverttoPDF(Filepath);
                        if (converteredTXT_Word == "Ошибка конертации" || converteredTXT_Word == "Файл слишком большой для конвертации")
                        {
                            await _botclient.SendTextMessageAsync(
                                chatId: ChatId,
                                text: $"❌ {converteredTXT_Word}",
                                cancellationToken: cancellation
                            );
                            return;
                        }
                        if (session.Email != null)
                        {
                            SendEmail.Send senn = new SendEmail.Send();
                            await senn.SmptServerSend(session.Email, converteredTXT_Word);
                        }
                        await Task.Delay(300);

                        SendDocument senddocumenttxt_word = new SendDocument(_botclient, _logger, _userSession);
                        await senddocumenttxt_word.SendDocumentToChatAsync(ChatId, converteredTXT_Word, cancellation);

                        DocumentDowloaded doc = new DocumentDowloaded(_botclient, _userSession, _logger);                      
                        doc.TryDeleteFile(converteredTXT_Word);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Не удалось получить конвертировать из pdf в word" + ex.Message);
                }
            }
        }


        public class Default: Formats
        {
            public async Task CurrentFormats(string formalower, long ChatId, string Filepath, CancellationToken cancellation)
            {
                Console.WriteLine("Отсутствие формата");
            }
        }


        public class SimpleFactory
        {
            public readonly ITelegramBotClient _botclient;
            public readonly Dictionary<long, Models.UserSession> _userSession;
            public readonly Microsoft.Extensions.Logging.ILogger _logger;

            public SimpleFactory(ITelegramBotClient botClient,
                 Dictionary<long, Models.UserSession> userSession,
                  Microsoft.Extensions.Logging.ILogger logger)
            {
                _botclient = botClient;
                _userSession = userSession;
                _logger = logger;
            }

            public  Formats createProduct(string formalower, long ChatId, string Filepath, CancellationToken cancellation)
            {
                switch (formalower.ToLower())
                {
                case "pdf": return new PDFtoWord(_botclient, _userSession, _logger);
                case "txt": return new TEXTtoWord(_botclient, _userSession, _logger);
                case "docx": return new WORDtoPDF(_botclient, _userSession, _logger);
                case "doc": return new WORDtoPDF(_botclient, _userSession, _logger);
                default: return new Default();
                }           
            }
        } 
}
