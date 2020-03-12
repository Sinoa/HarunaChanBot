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
using Firebase.Database.Query;
using HarunaChanBot.Framework;

namespace HarunaChanBot.Services
{
    public class MessageLoggingService : ApplicationService
    {
        public const string BasePath = "ChannelMessageLog";

        private Dictionary<string, ChildQuery> queryTable;
        private Queue<ChannelMessageLog> logQueue;
        private Task sendTask;



        public MessageLoggingService()
        {
            queryTable = new Dictionary<string, ChildQuery>();
            logQueue = new Queue<ChannelMessageLog>();
        }


        protected internal override void Update()
        {
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                var messageLog = new ChannelMessageLog();
                messageLog.ChannelName = message.Channel.Name;
                messageLog.Timestamp = message.Timestamp.ToOffset(TimeSpan.FromHours(9.0));
                messageLog.UserName = message.Author.Username;
                messageLog.UserID = message.Author.Id;
                messageLog.IsBot = message.Author.IsBot;
                messageLog.MessageText = message.Content;


                logQueue.Enqueue(messageLog);
            }


            if (logQueue.Count == 0)
            {
                return;
            }


            if (sendTask != null && !sendTask.IsCompleted)
            {
                return;
            }


            var log = logQueue.Dequeue();
            if (!queryTable.TryGetValue(log.ChannelName, out var query))
            {
                query = Application.Current.GetService<DatabaseService>().CreateQuery($"{BasePath}/{log.ChannelName}");
                queryTable[log.ChannelName] = query;
            }


            sendTask = query.PutAsync(log);
        }


        protected internal override void Terminate()
        {
            foreach (var queryKV in queryTable)
            {
                queryKV.Value.Dispose();
            }


            queryTable.Clear();
        }
    }



    public class ChannelMessageLog
    {
        public string ChannelName { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string UserName { get; set; }
        public ulong UserID { get; set; }
        public bool IsBot { get; set; }
        public string MessageText { get; set; }
    }
}