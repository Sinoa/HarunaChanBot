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
using Newtonsoft.Json;

namespace HarunaChanBot.Services
{
    public class PaifuDetectService : ApplicationService
    {
        private static readonly Regex StampRegex = new Regex("<:([a-zA-Z0-9_-]+):([0-9]+)>");
        private static readonly Regex MentionRegex = new Regex("<@!?([0-9]+)>");
        private static readonly Regex PaifuUrlRegex = new Regex("https://game.mahjongsoul.com/\\?paipu=[0-9a-z_-]+");

        private readonly PaifuDetectServiceData serviceData;
        private SocketTextChannel targetChannel;
        private ulong allMessageReceiveChannelID;
        private List<SocketTextChannel> kokutiTargetChannelList;



        public PaifuDetectService()
        {
            kokutiTargetChannelList = new List<SocketTextChannel>();
            serviceData = PaifuDetectServiceData.Load();
            targetChannel = null;
        }


        protected internal override void Terminate()
        {
            serviceData.Save();
        }


        protected internal override void OnGuildAvailable(SocketGuild guild)
        {
            targetChannel = ApplicationMain.Current.GetTextChannel(serviceData.NotifyChannelID);
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


            FreeRunUpdate();
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


        private void FreeUpdate(SocketMessage message)
        {
            UpdateKokuti(message);


            if (!ContainPaifuUrl(message.Content) || serviceData.PaifuPasteChannelID != message.Channel.Id || targetChannel == null)
            {
                return;
            }


            var isKentou = message.Content.Contains("検討をお願いします");
            var pasterName = message.Author.Username;
            string messageText;


            if (isKentou)
            {
                messageText = $"{pasterName} さんが、牌譜検討して欲しい牌譜を貼ったみたいよ。見てあげてちょうだいね。";
            }
            else
            {
                messageText = $"{pasterName} さんが、牌譜を貼ったみたいよ。どんな牌譜か気になるわね。";
            }


            ApplicationMain.Current.Post.SendMessage(messageText, targetChannel);
        }


        private void FreeRunUpdate()
        {
        }


        private void UpdateKokuti(SocketMessage message)
        {
            if (allMessageReceiveChannelID != message.Channel.Id)
            {
                return;
            }


            allMessageReceiveChannelID = 0;
            foreach (var channel in kokutiTargetChannelList)
            {
                ApplicationMain.Current.Post.SendMessage(message.Content, channel);
            }


            kokutiTargetChannelList.Clear();
            ApplicationMain.Current.Post.SendMessage("全体への告知が完了しました。", message.Channel);
        }


        private void ProcessCommand(SocketMessage message, string command, string[] arguments)
        {
            switch (command)
            {
                case "牌譜通知はここにお願いします": SetNotifyTargetChannel(message, arguments); return;
                case "牌譜を貼る場所はここです": SetPaifuPasteChannel(message, arguments); return;
                case "管理者の追加": AddAdminUser(message, arguments); return;
                case "全体へ告知の準備": StandbySendAllChannel(message, arguments); return;
                case "告知対象に追加": AddTeamChannel(message, arguments); return;
            }
        }


        private void AddTeamChannel(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            var isAdminUser = serviceData.adminUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor || isAdminUser))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            serviceData.teamChannelList.Add(message.Channel.Id);
            serviceData.Save();
            ApplicationMain.Current.Post.SendMessage("Success...", message.Channel);
        }


        private void StandbySendAllChannel(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            var isAdminUser = serviceData.adminUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor || isAdminUser))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            var buffer = new StringBuilder();
            foreach (var channelID in serviceData.teamChannelList)
            {
                var channel = ApplicationMain.Current.GetTextChannel(channelID);
                if (channel != null)
                {
                    kokutiTargetChannelList.Add(channel);
                    buffer.Append($"{channel.Name}\n");
                }
            }


            allMessageReceiveChannelID = message.Channel.Id;
            ApplicationMain.Current.Post.SendMessage($"{buffer} 以上の全体告知の準備が出来ました", message.Channel);
        }


        private void AddAdminUser(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            var isAdminUser = serviceData.adminUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor || isAdminUser))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            if (arguments.Length < 1)
            {
                ApplicationMain.Current.Post.SendMessage($"argument error...", message.Channel);
                return;
            }


            if (!ulong.TryParse(arguments[0], out var id))
            {
                ApplicationMain.Current.Post.SendMessage("id out of range error....", message);
                return;
            }


            serviceData.adminUserIDList.Add(id);
            serviceData.Save();
            ApplicationMain.Current.Post.SendMessage("Set completed...", message.Channel);
        }


        private void SetNotifyTargetChannel(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            serviceData.NotifyChannelID = message.Channel.Id;
            serviceData.Save();
            ApplicationMain.Current.Post.SendMessage("わかったわ、牌譜が貼られたらここに通知するわね", message.Channel);
        }


        private void SetPaifuPasteChannel(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            serviceData.PaifuPasteChannelID = message.Channel.Id;
            serviceData.Save();
            ApplicationMain.Current.Post.SendMessage("わかったわ、ここに牌譜が貼られたか確認するわ", message.Channel);
        }


        private bool ContainPaifuUrl(string messageText)
        {
            return PaifuUrlRegex.IsMatch(messageText);
        }


        private string RemoveMentionContent(string content)
        {
            return MentionRegex.Replace(content, string.Empty);
        }
    }



    public class PaifuDetectServiceData
    {
        private const string FileName = "paifuDetectService.json";

        public ulong NotifyChannelID;
        public ulong PaifuPasteChannelID;
        public List<ulong> teamChannelList;
        public List<ulong> adminUserIDList;



        public static PaifuDetectServiceData Load()
        {
            if (!File.Exists(FileName))
            {
                return new PaifuDetectServiceData();
            }


            var jsonData = File.ReadAllText(FileName);
            var data = JsonConvert.DeserializeObject<PaifuDetectServiceData>(jsonData);
            data.Validate();
            return data;
        }


        public void Save()
        {
            var jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FileName, jsonData);
        }


        public void Validate()
        {
            teamChannelList ??= new List<ulong>();
            adminUserIDList ??= new List<ulong>();
        }
    }
}