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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using HarunaChanBot.Framework;
using MersenneTwister;
using Newtonsoft.Json;

namespace HarunaChanBot.Services
{
    public class FreedomService : ApplicationService
    {
        private static readonly Regex StampRegex = new Regex("<:([a-zA-Z0-9_-]+):([0-9]+)>");
        private static readonly Regex MentionRegex = new Regex("<@!?([0-9]+)>");

        private FreedomData freedomData;
        private DateTimeOffset nextTimeSignalTime;
        private SocketTextChannel timeSignalTargetChannel;
        private SocketTextChannel shoutTargetChannel;
        private Random random;



        public FreedomService()
        {
            freedomData = FreedomData.Load();
            freedomData.VerifyAndRepair();


            var tmp = DateTimeOffset.Now.AddHours(1.0);
            nextTimeSignalTime = new DateTimeOffset(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, 0, 0, TimeSpan.FromHours(9.0));


            random = DsfmtRandom.Create();
        }


        protected internal override void Update()
        {
            var botUserID = ApplicationMain.Current.Config.DiscordBotID;
            foreach (var message in ApplicationMain.Current.Post.ReceivedMessageList)
            {
                if (message.Author.IsBot) continue;


                var isMyMention = message.MentionedUsers.Any(x => x.Id == botUserID);
                if (isMyMention)
                {
                    Update(message);
                }
                else
                {
                    FreeUpdate(message);
                }
            }


            UpdateTimeSignal();
        }


        protected internal override void OnGuildAvailable(SocketGuild guild)
        {
            timeSignalTargetChannel = ApplicationMain.Current.GetTextChannel(freedomData.TimeSignalTargetChannelID);
            shoutTargetChannel = ApplicationMain.Current.GetTextChannel(freedomData.ShoutTargetChannelID);
        }


        protected internal override void Terminate()
        {
            freedomData.Save();
        }


        private void UpdateTimeSignal()
        {
            if (timeSignalTargetChannel == null)
            {
                return;
            }


            if (nextTimeSignalTime > DateTimeOffset.Now)
            {
                return;
            }


            var mentionMessageBuffer = new StringBuilder();
            foreach (var targetUserID in freedomData.TimeSignalNotifyTargetUserIDTable[nextTimeSignalTime.Hour])
            {
                mentionMessageBuffer.Append($"<@{targetUserID}>");
            }
            if (mentionMessageBuffer.Length > 0)
            {
                mentionMessageBuffer.Append("\n");
            }


            var timeSignalMessage = freedomData.GetTimeSignalMessage(random, nextTimeSignalTime.Hour);
            ApplicationMain.Current.Post.SendMessage($"{mentionMessageBuffer}{nextTimeSignalTime.Hour}時です。\n{timeSignalMessage}", timeSignalTargetChannel);


            nextTimeSignalTime = nextTimeSignalTime.AddHours(1.0);
            nextTimeSignalTime = new DateTimeOffset(
                nextTimeSignalTime.Year, nextTimeSignalTime.Month, nextTimeSignalTime.Day,
                nextTimeSignalTime.Hour, 0, 0,
                TimeSpan.FromHours(9.0));


            freedomData.Save();
        }


        private void FreeUpdate(SocketMessage message)
        {
            if (message.Channel.Id == freedomData.ShoutTargetChannelID)
            {
                var stamps = GetStampIDs(message.Content);
                foreach (var stampID in stamps)
                {
                    if (freedomData.StampSensorList.Contains(stampID))
                    {
                        var reactiveMessage = freedomData.GetReactiveMessage(random);
                        if (reactiveMessage == null) return;
                        ApplicationMain.Current.Post.SendMessage(reactiveMessage, message.Channel);
                        return;
                    }
                }
            }
        }


        private void Update(SocketMessage message)
        {
            var mentionRemovedContent = RemoveMentionContent(message.Content).Trim();
            if (mentionRemovedContent == "Kill")
            {
                var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
                var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
                if (!(isSupervisor || isSubSupervisor))
                {
                    ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                }
                else
                {
                    ApplicationMain.Current.Post.SendMessage("Exit...", message.Channel);
                    ApplicationMain.Current.Quit();
                    return;
                }
            }


            var splitedContent = mentionRemovedContent.Replace("　", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = splitedContent[0];
            var arguments = new string[splitedContent.Length - 1];
            for (int i = 1; i < splitedContent.Length; ++i)
            {
                arguments[i - 1] = splitedContent[i];
            }


            ProcessCommand(message, command, arguments);
        }


        private void ProcessCommand(SocketMessage message, string command, string[] arguments)
        {
            switch (command)
            {
                case "スケジュールの登録": AddSchedule(message, arguments); return;
                case "スケジュールの確認": ShowSchedule(message, arguments); return;
                case "時報はここにお願い": SetTimeSignalChannel(message, arguments); return;
                case "時報の登録": SetTimeSignalMessage(message, arguments); return;
                case "時報の一覧": ShowTimeSignalMessage(message, arguments); return;
                case "時報の削除": RemoveTimeSignalMessage(message, arguments); return;
                case "反応するスタンプの登録": AddStamp(message, arguments); return;
                case "反応するメッセージの登録": AddReactiveMessage(message, arguments); return;
                case "反応するメッセージの一覧": ShowReactiveMessage(message, arguments); return;
                case "反応するメッセージの削除": RemoveReactiveMessage(message, arguments); return;
                case "副管理者の登録": AddSubSupervisorUserID(message, arguments); return;
                case "副管理者の解除": RemoveSubSupervisorUserID(message, arguments); return;
                case "記憶して": DoConfigSave(message, arguments); return;
                case "時報の通知をお願い": AddTimeSignalNotify(message, arguments); return;
                case "時報の通知を止めて": StopTimeSignalNotify(message, arguments); return;
                case "ここで喘いで": SetShoutTargetChannel(message, arguments); return;
            }
        }


        private void SetShoutTargetChannel(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            freedomData.ShoutTargetChannelID = message.Channel.Id;
            shoutTargetChannel = message.Channel as SocketTextChannel;
            ApplicationMain.Current.Post.SendMessage("Set completed...", message);
        }


        private void RemoveReactiveMessage(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 1)
            {
                ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                return;
            }


            if (!int.TryParse(arguments[0], out var id))
            {
                ApplicationMain.Current.Post.SendMessage("id out of range error....", message);
                return;
            }


            if (id < 0 || id >= freedomData.ReactiveMessageList.Count)
            {
                ApplicationMain.Current.Post.SendMessage("id out of range error....", message);
                return;
            }


            freedomData.ReactiveMessageList.RemoveAt(id);
            ApplicationMain.Current.Post.SendMessage("Remove completed...", message);
        }


        private void ShowReactiveMessage(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            var list = freedomData.ReactiveMessageList;
            var buffer = new StringBuilder();
            for (int i = 0; i < list.Count; ++i)
            {
                buffer.Append($"[{i}]{list[i]}\n");
            }


            ApplicationMain.Current.Post.SendMessage($"{buffer}", message.Channel);
        }


        private void AddTimeSignalNotify(SocketMessage message, string[] arguments)
        {
            if (arguments.Length >= 1)
            {
                if (!int.TryParse(arguments[0], out var time))
                {
                    ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                    return;
                }


                if (time < 0 || time > 24)
                {
                    ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                    return;
                }


                freedomData.TimeSignalNotifyTargetUserIDTable[time].Add(message.Author.Id);
            }
            else
            {
                for (int i = 0; i < 25; ++i)
                {
                    freedomData.TimeSignalNotifyTargetUserIDTable[i].Add(message.Author.Id);
                }
            }


            ApplicationMain.Current.Post.SendMessage("Set completed...", message.Channel);
        }


        private void StopTimeSignalNotify(SocketMessage message, string[] arguments)
        {
            if (arguments.Length >= 1)
            {
                if (!int.TryParse(arguments[0], out var time))
                {
                    ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                    return;
                }


                if (time < 0 || time > 24)
                {
                    ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                    return;
                }


                freedomData.TimeSignalNotifyTargetUserIDTable[time].Remove(message.Author.Id);
            }
            else
            {
                for (int i = 0; i < 25; ++i)
                {
                    freedomData.TimeSignalNotifyTargetUserIDTable[i].Remove(message.Author.Id);
                }
            }


            ApplicationMain.Current.Post.SendMessage("Remove completed...", message.Channel);
        }


        private void DoConfigSave(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            freedomData.Save();
            ApplicationMain.Current.Post.SendMessage("Save completed...", message.Channel);
        }


        private void RemoveSubSupervisorUserID(SocketMessage message, string[] arguments)
        {
            if (message.Author.Id != ApplicationMain.Current.Config.SupervisorUserID)
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 1)
            {
                ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                return;
            }


            if (!ulong.TryParse(arguments[0], out var subSupervisorUserID))
            {
                ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                return;
            }


            ApplicationMain.Current.Config.SubSupervisorUserIDList.Remove(subSupervisorUserID);
            ApplicationMain.Current.Post.SendMessage("Remove completed...", message.Channel);
        }


        private void AddSubSupervisorUserID(SocketMessage message, string[] arguments)
        {
            if (message.Author.Id != ApplicationMain.Current.Config.SupervisorUserID)
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 1)
            {
                ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                return;
            }


            if (!ulong.TryParse(arguments[0], out var subSupervisorUserID))
            {
                ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                return;
            }


            ApplicationMain.Current.Config.SubSupervisorUserIDList.Add(subSupervisorUserID);
            ApplicationMain.Current.Post.SendMessage("Add completed...", message.Channel);
        }


        private void AddReactiveMessage(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 1)
            {
                ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                return;
            }



            if (freedomData.ReactiveMessageList.Contains(arguments[0]))
            {
                ApplicationMain.Current.Post.SendMessage($"this message exists...", message.Channel);
                return;
            }


            freedomData.ReactiveMessageList.Add(arguments[0]);
            ApplicationMain.Current.Post.SendMessage("Set completed...", message.Channel);
        }


        private void AddStamp(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 1)
            {
                ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                return;
            }


            var match = StampRegex.Match(arguments[0]);
            if (!match.Success)
            {
                ApplicationMain.Current.Post.SendMessage($"not stamp....", message.Channel);
                return;
            }


            if (!ulong.TryParse(match.Groups[2].Value, out var id))
            {
                ApplicationMain.Current.Post.SendMessage($"parse error...", message.Channel);
                return;
            }


            freedomData.StampSensorList.Add(id);
            ApplicationMain.Current.Post.SendMessage("Set completed...", message.Channel);
        }


        private void SetTimeSignalMessage(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 2)
            {
                ApplicationMain.Current.Post.SendMessage("Argument error....", message);
                return;
            }


            if (!int.TryParse(arguments[0], out var hour))
            {
                ApplicationMain.Current.Post.SendMessage("hour out of range error....", message);
                return;
            }


            if (hour < 0 || hour > 24)
            {
                ApplicationMain.Current.Post.SendMessage($"hour out of range error.... not supported {hour}", message);
                return;
            }

            freedomData.TimeSignalMessageTable[hour].Add(arguments[1]);
            ApplicationMain.Current.Post.SendMessage("Set completed...", message.Channel);
        }


        private void ShowTimeSignalMessage(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 1)
            {
                ApplicationMain.Current.Post.SendMessage("Argument error....", message);
                return;
            }


            if (!int.TryParse(arguments[0], out var hour))
            {
                ApplicationMain.Current.Post.SendMessage("hour out of range error....", message);
                return;
            }


            if (hour < 0 || hour > 24)
            {
                ApplicationMain.Current.Post.SendMessage($"hour out of range error.... not supported {hour}", message);
                return;
            }


            var buffer = new StringBuilder();
            var list = freedomData.TimeSignalMessageTable[hour];
            buffer.Append($"{hour}時の一覧は以下の通りです\n");
            for (int i = 0; i < list.Count; ++i)
            {
                var signalMessage = list[i];
                buffer.Append($"[{i}]{signalMessage}\n");
            }


            ApplicationMain.Current.Post.SendMessage(buffer.ToString(), message.Channel);
        }


        private void RemoveTimeSignalMessage(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 2)
            {
                ApplicationMain.Current.Post.SendMessage("Argument error....", message);
                return;
            }


            if (!int.TryParse(arguments[0], out var hour))
            {
                ApplicationMain.Current.Post.SendMessage("hour out of range error....", message);
                return;
            }


            if (!int.TryParse(arguments[1], out var index))
            {
                ApplicationMain.Current.Post.SendMessage("index parse error....", message);
                return;
            }


            if (hour < 0 || hour > 24)
            {
                ApplicationMain.Current.Post.SendMessage($"hour out of range error.... not supported {hour}", message);
                return;
            }

            freedomData.TimeSignalMessageTable[hour].RemoveAt(index);
            ApplicationMain.Current.Post.SendMessage("Remove completed...", message.Channel);
        }


        private void SetTimeSignalChannel(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            freedomData.TimeSignalTargetChannelID = message.Channel.Id;
            timeSignalTargetChannel = message.Channel as SocketTextChannel;
            ApplicationMain.Current.Post.SendMessage("Set completed...", timeSignalTargetChannel);
        }


        private void AddSchedule(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 4)
            {
                ApplicationMain.Current.Post.SendMessage("Argument error...", message.Channel);
                return;
            }


            var jobData = new JobData();
            jobData.ID = freedomData.nextID++;
            jobData.Active = true;


            jobData.IntervalType = (arguments[0]) switch
            {
                "毎時" => JobIntervalType.Hourly,
                "毎日" => JobIntervalType.Daily,
                "毎週" => JobIntervalType.Weekly,
                "毎月" => JobIntervalType.Monthly,
                "毎年" => JobIntervalType.Yearly,
                _ => JobIntervalType.OneTime,
            };


            DateTimeOffset.TryParse($"{arguments[1]} {arguments[2]}", out jobData.ScheduledTimeOffset);


            jobData.Message = arguments[3];
            freedomData.JobList.Add(jobData);
            ApplicationMain.Current.Post.SendMessage($"Added... JobID={jobData.ID}", message.Channel);
        }


        private void ShowSchedule(SocketMessage message, string[] arguments)
        {
            var buffer = new StringBuilder();
            buffer.Append("```");
            foreach (var jobData in freedomData.JobList)
            {
                buffer.Append($"アクティブ={jobData.Active} ID={jobData.ID} Message={jobData.Message} Schedule={jobData.IntervalType} {jobData.ScheduledTimeOffset}\n");
            }
            buffer.Append("```");


            ApplicationMain.Current.Post.SendMessage(buffer.ToString(), message.Channel);
        }


        private string RemoveMentionContent(string content)
        {
            return MentionRegex.Replace(content, string.Empty);
        }


        private ulong[] GetStampIDs(string content)
        {
            var matches = StampRegex.Matches(content);
            var results = new ulong[matches.Count];
            for (int i = 0; i < matches.Count; ++i)
            {
                ulong.TryParse(matches[i].Groups[2].Value, out results[i]);
            }


            return results;
        }
    }



    public class FreedomData
    {
        public ulong nextID;
        public List<JobData> JobList;
        public ulong TimeSignalTargetChannelID;
        public ulong ShoutTargetChannelID;
        public Dictionary<int, List<string>> TimeSignalMessageTable;
        public HashSet<ulong> StampSensorList;
        public List<string> ReactiveMessageList;
        public Dictionary<int, HashSet<ulong>> TimeSignalNotifyTargetUserIDTable;



        public static FreedomData Load()
        {
            if (!File.Exists("freedom.json"))
            {
                var newData = new FreedomData();
                newData.JobList = new List<JobData>();
                newData.TimeSignalMessageTable = new Dictionary<int, List<string>>();
                newData.StampSensorList = new HashSet<ulong>();
                newData.ReactiveMessageList = new List<string>();
                newData.TimeSignalNotifyTargetUserIDTable = new Dictionary<int, HashSet<ulong>>();
                for (int i = 0; i < 25; ++i)
                {
                    newData.TimeSignalMessageTable[i] = new List<string>();
                    newData.TimeSignalNotifyTargetUserIDTable[i] = new HashSet<ulong>();
                }
                return newData;
            }


            var jsonData = File.ReadAllText("freedom.json");
            return JsonConvert.DeserializeObject<FreedomData>(jsonData);
        }


        public void VerifyAndRepair()
        {
            JobList ??= new List<JobData>();
            TimeSignalMessageTable ??= new Dictionary<int, List<string>>();
            StampSensorList ??= new HashSet<ulong>();
            ReactiveMessageList ??= new List<string>();
            TimeSignalNotifyTargetUserIDTable ??= new Dictionary<int, HashSet<ulong>>();
            for (int i = 0; i < 25; ++i)
            {
                if (!TimeSignalMessageTable.ContainsKey(i))
                {
                    TimeSignalMessageTable[i] = new List<string>();
                }


                if (!TimeSignalNotifyTargetUserIDTable.ContainsKey(i))
                {
                    TimeSignalNotifyTargetUserIDTable[i] = new HashSet<ulong>();
                }
            }
        }


        public void Save()
        {
            File.WriteAllText("freedom.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }


        public string GetTimeSignalMessage(Random random, int hour)
        {
            var list = TimeSignalMessageTable[hour];
            if (list.Count == 0)
            {
                return string.Empty;
            }


            return list[random.Next(0, list.Count)];
        }


        public string GetReactiveMessage(Random random)
        {
            if (ReactiveMessageList.Count == 0)
            {
                return null;
            }


            return ReactiveMessageList[random.Next(0, ReactiveMessageList.Count)];
        }
    }


    public enum JobIntervalType : int
    {
        OneTime = 0,
        Yearly = 1,
        Monthly = 2,
        Weekly = 3,
        Daily = 4,
        Hourly = 5,
    }


    public class JobData
    {
        public ulong ID;
        public bool Active;
        public DateTimeOffset ScheduledTimeOffset;
        public JobIntervalType IntervalType;
        public string Message;
    }
}