// zlib/libpng License
//
// Copyright(c) 2020 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using HarunaChanBot.BotCommands;

namespace HarunaChanBot
{
    public class BotClient
    {
        private static readonly string[] CommandHeaderTexts;
        private static readonly Dictionary<string, BotCommand> CommandTable;

        private TaskCompletionSource<int> completionSource;
        private DiscordSocketClient client;
        private ISocketMessageChannel specifyTargetMessageChannel;



        static BotClient()
        {
            CommandHeaderTexts = new string[] { "陽菜ちゃん、", "お願い陽菜ちゃん、", "はるなちゃん、", "おねがいはるなちゃん、", "!HB:" };
            CommandTable = new Dictionary<string, BotCommand>()
            {
                { "家に帰って", new LogoutCommand() },
                { "レベルを教えて", new ShowLevelsCommand() },
                { "URLを教えて", new ShowUrls() },
                { "おやすみなさい", new ShowOyasumiCommand() },
                { "おやすみ", new ShowOyasumiCommand() },
                { "おやすみなんし", new ShowOyasumiCommand() },
                { "お休みなさい", new ShowOyasumiCommand() },
                { "おやすみなしあ", new ShowOyasumiCommand() },
                { "おはよう", new ShowOhayoCommand() },
                { "おはよ", new ShowOhayoCommand() },
                { "おはようございます", new ShowOhayoCommand() },
                { "おはYostar", new ShowOhayoCommand() },
                { "おはYostar!", new ShowOhayoCommand() },
                { "おはYostar!!", new ShowOhayoCommand() },
                { "おはよーすたー", new ShowOhayoCommand() },
                { "おはよーすたー！", new ShowOhayoCommand() },
                { "おはよーすたー！！", new ShowOhayoCommand() },
            };
        }


        public BotClient()
        {
            completionSource = new TaskCompletionSource<int>();
            client = new DiscordSocketClient();
            client.MessageReceived += OnMessageReceived;
        }


        public async Task<int> DoRunLoop(string token)
        {
            try
            {
                Console.WriteLine("login discord...");
                await client.LoginAsync(Discord.TokenType.Bot, token);
                Console.WriteLine("wakeup bot service...");
                await client.StartAsync();
                Console.WriteLine("Welcome!! bot service!!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Login failed...");
                Console.WriteLine($"{e.Message}");
                completionSource.SetResult(-1);
            }


            var result = await completionSource.Task;


            Console.WriteLine("logout discord...");
            await client.LogoutAsync();
            Console.WriteLine("logouted discord...");


            return result;
        }


        private Task OnMessageReceived(SocketMessage arg)
        {
            Console.WriteLine($"{arg.Timestamp.ToString().PadRight(30)}IsBot={arg.Author.IsBot} UserName={arg.Author.Username} message={arg.Content}");


            if (arg.Author.IsBot || !ParseMessageCommand(arg.Content, out var commandText, out var argument))
            {
                return Task.CompletedTask;
            }


            Console.WriteLine($"Command={commandText} Argument={argument}");


            if (!CommandTable.TryGetValue(commandText, out var command))
            {
                Console.WriteLine($"CommandNotFound:{commandText}");
                ReplyMessage(CreateCommandNotFoundMessage(commandText, arg), new BotCommandContext(this, arg, argument));
                return Task.CompletedTask;
            }


            if (!command.IsPermittedUser(arg.Author.Username))
            {
                Console.WriteLine($"CommandNotPermitted:{commandText} Name:{arg.Author.Username}");
                ReplyMessage(CreateNotPermittedMessage(arg), new BotCommandContext(this, arg, argument));
                return Task.CompletedTask;
            }


            return command.RunCommand(new BotCommandContext(this, arg, argument));
        }


        private bool ParseMessageCommand(string message, out string command, out string argument)
        {
            command = null;
            argument = null;
            foreach (var commandHeaderText in CommandHeaderTexts)
            {
                if (message.StartsWith(commandHeaderText))
                {
                    var splitedMessage = SplitMessage(message.Remove(0, commandHeaderText.Length));
                    command = splitedMessage[0];
                    if (splitedMessage.Length > 1)
                    {
                        argument = splitedMessage[1];
                    }
                    break;
                }
            }


            return command != null;
        }


        private string[] SplitMessage(string message)
        {
            var stringList = new List<string>();
            var charaList = new List<char>();
            foreach (var chara in message)
            {
                if (char.IsWhiteSpace(chara))
                {
                    stringList.Add(new string(charaList.ToArray()));
                    charaList.Clear();
                    continue;
                }


                charaList.Add(chara);
            }


            if (stringList.Count == 0)
            {
                return new string[] { message };
            }


            if (charaList.Count != 0)
            {
                stringList.Add(new string(charaList.ToArray()));
            }


            return stringList.ToArray();
        }


        private string CreateCommandNotFoundMessage(string commandName, SocketMessage arg)
        {
            var messageTemplate = ApplicationMain.Context.Config.CommandNotFoundMessageTemplate;
            var message = messageTemplate.Replace("%CMD_NAME%", commandName);
            return message;
        }


        private string CreateNotPermittedMessage(SocketMessage arg)
        {
            var messageTemplate = ApplicationMain.Context.Config.NotPermittedMessageTemplate;
            var message = messageTemplate.Replace("%SV_NAME%", ApplicationMain.Context.Config.SupervisorName);
            return message;
        }


        public void SetSpecifyTargetMessageChannel(ISocketMessageChannel channel)
        {
            specifyTargetMessageChannel = channel;
        }


        public Task Logout()
        {
            completionSource.SetResult(0);
            return Task.CompletedTask;
        }


        public Task SendMessage(string message, BotCommandContext context)
        {
            return context.ReceiveSocketMessage.Channel.SendMessageAsync(message);
        }


        public Task ReplyMessage(string message, BotCommandContext context)
        {
            return context.ReceiveSocketMessage.Channel.SendMessageAsync($"{context.ReceiveSocketMessage.Author.Mention} {message}");
        }


        public Task SendSpecifyChannelMessage(string message, BotCommandContext context)
        {
            if (specifyTargetMessageChannel == null)
            {
                Console.WriteLine("SpecifyTargetMessageChannel is null.");
                return Task.CompletedTask;
            }


            return specifyTargetMessageChannel.SendMessageAsync(message);
        }


        public Task ReplySpecifyChannelMessage(string message, BotCommandContext context)
        {
            if (specifyTargetMessageChannel == null)
            {
                Console.WriteLine("SpecifyTargetMessageChannel is null.");
                return Task.CompletedTask;
            }


            return specifyTargetMessageChannel.SendMessageAsync($"{context.ReceiveSocketMessage.Author.Mention} {message}");
        }
    }
}
