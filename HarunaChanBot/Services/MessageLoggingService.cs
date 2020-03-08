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
using System.Diagnostics;
using System.IO;
using System.Text;
using Discord.WebSocket;
using HarunaChanBot.Framework;

namespace HarunaChanBot.Services
{
    public class MessageLoggingService : ApplicationService
    {
        private Stopwatch stopwatch;
        private StreamWriter writer;
        private bool dirty;



        public MessageLoggingService()
        {
            if (!Directory.Exists("Log"))
            {
                Directory.CreateDirectory("Log");
            }


            writer = new StreamWriter("Log/message.log", true, new UTF8Encoding(false));
            stopwatch = Stopwatch.StartNew();
        }


        protected internal override void Update()
        {
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                dirty = true;
                AddLog(message);
            }


            if (stopwatch.ElapsedMilliseconds >= 30 * 1000 && dirty)
            {
                dirty = false;
                writer.Flush();
                stopwatch.Restart();
            }
        }


        protected internal override void Terminate()
        {
            writer.Dispose();
        }


        private void AddLog(SocketMessage message)
        {
            var timestamp = message.Timestamp.ToOffset(TimeSpan.FromHours(9.0));
            var messageText = message.Content;
            var channelName = message.Channel.Name;
            var userName = message.Author.Username;
            var userID = message.Author.Id;
            var isBot = message.Author.IsBot;
            var logText = $"[{timestamp}]\nChannel={channelName}\nUserName={userName}({userID})\nIsBot={isBot}\n{messageText}\n\n";
            writer.Write(logText);


            Console.Write(logText);
        }
    }
}