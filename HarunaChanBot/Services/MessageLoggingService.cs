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
using System.Diagnostics;
using System.Linq;
using Discord.WebSocket;
using HarunaChanBot.Framework;

namespace HarunaChanBot.Services
{
    public class MessageLoggingService : ApplicationService
    {
        private Stopwatch stopwatch;
        private List<MessageLog> messageList;
        private List<UserInfo> nameInfo;
        private List<MessageAttachement> attachment;



        public MessageLoggingService()
        {
            stopwatch = Stopwatch.StartNew();
            messageList = new List<MessageLog>(4 << 10);
            nameInfo = new List<UserInfo>();
            attachment = new List<MessageAttachement>();
        }


        protected internal override void Update()
        {
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                messageList.Add(ToLog(message));
            }


            if (stopwatch.ElapsedMilliseconds >= 30 * 1000 && messageList.Count > 0)
            {
                TranslateMessage();
                stopwatch.Restart();
            }
        }


        private void TranslateMessage()
        {
            using var database = new DatabaseContext();

            foreach (var message in messageList)
            {
                database.Add(message);
            }


            foreach (var att in attachment)
            {
                database.Add(att);
            }


            foreach (var info in nameInfo)
            {
                var i = database.UserInfo.FirstOrDefault(x => x.DiscordID == info.DiscordID);
                if (i == null)
                {
                    database.Add(info);
                    continue;
                }


                i.LastActiveTimestamp = info.LastActiveTimestamp;
            }

            database.SaveChanges();


            messageList.Clear();
            attachment.Clear();
            nameInfo.Clear();
        }


        private MessageLog ToLog(SocketMessage message)
        {
            if (nameInfo.Find(x => x.DiscordID == message.Author.Id) == null)
            {
                nameInfo.Add(new UserInfo()
                {
                    DiscordID = message.Author.Id,
                    Name = message.Author.Username,
                    LastActiveTimestamp = DateTimeOffset.Now,
                });
            }


            var messageLog = new MessageLog();
            messageLog.PostTimestamp = message.Timestamp.ToOffset(TimeSpan.FromHours(9.0));
            messageLog.Message = message.Content;
            messageLog.MessageID = message.Id;
            messageLog.UserID = message.Author.Id;
            messageLog.ChannelID = message.Channel.Id;
            messageLog.IsBot = message.Author.IsBot;


            attachment.AddRange(
                message.Attachments
                .Select(x => new MessageAttachement()
                {
                    MessageID = message.Id,
                    ChannelID = message.Channel.Id,
                    AttachmentID = x.Id,
                    FileName = x.Filename,
                    AttachmentURL = x.Url,
                })
            );


            return messageLog;
        }
    }
}