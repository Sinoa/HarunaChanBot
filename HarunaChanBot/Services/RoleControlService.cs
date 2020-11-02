using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using Discord.WebSocket;
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;
using LiteDB;

namespace HarunaChanBot.Services
{
    public class RoleControlService : ApplicationService
    {
        private static readonly string DatabaseName = "club.db";
        private static readonly string CollectionName = "club";

        private Dictionary<string, ulong> clubIDTable = new Dictionary<string, ulong>();



        public RoleControlService()
        {
            using var database = new LiteDatabase(DatabaseName);
            var collection = database.GetCollection<RoleRecord>(CollectionName);

            clubIDTable.Clear();
            foreach (var record in collection.Query().ToArray())
            {
                clubIDTable[record.ClubName] = record.RoleID;
            }
        }


        protected internal override void Update()
        {
            var service = Application.Current.GetService<HarunaChanQuestService>();
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                var playerData = service.GetPlayerData(message);
                if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments)) continue;

                switch (command)
                {
                    case "入部":
                        JoinClub(message, arguments ?? Array.Empty<string>(), playerData);
                        break;


                    case "退部":
                        LeaveClub(message, arguments ?? Array.Empty<string>(), playerData);
                        break;


                    case "部活一覧を見せて":
                        break;


                    case "部活ロールの設定":
                        SetupRole(message, arguments ?? Array.Empty<string>(), playerData);
                        break;


                    case "部活ロールの解除":
                        ClearRole(message, arguments ?? Array.Empty<string>(), playerData);
                        break;
                }
            }
        }


        private void JoinClub(SocketMessage message, string[] arguments, PlayerGameData playerData)
        {
            //Application.Current.Post.ReplyMessage($"{kaiwaData.GetOyasumiMessage()} また明日ね！", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void LeaveClub(SocketMessage message, string[] arguments, PlayerGameData playerData)
        {
            //Application.Current.Post.ReplyMessage($"{kaiwaData.GetOyasumiMessage()} また明日ね！", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void SetupRole(SocketMessage message, string[] arguments, PlayerGameData playerData)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (arguments.Length < 2)
            {
                Application.Current.Post.ReplyMessage("部活の設定をするのに情報が不足してるよ～。(部活名, ロールID)を教えて欲しいな", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var clubName = arguments[0];
            if (!ulong.TryParse(arguments[1], out var roleID))
            {
                Application.Current.Post.ReplyMessage("ロールのIDをちゃんと読めなかったよ～", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var guild = Application.Current.DiscordClient.GetGuild(message.Reference.GuildId.Value);
            var role = guild.GetRole(roleID);
            if (role == null)
            {
                Application.Current.Post.ReplyMessage($"ごめんなさい、ID'{roleID}'のロールが見つからなかったよ", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            clubIDTable[clubName] = roleID;


            using var database = new LiteDatabase(DatabaseName);
            var collection = database.GetCollection<RoleRecord>(CollectionName);
            var record = new RoleRecord();
            record.ClubName = clubName;
            record.RoleID = roleID;
            collection.Upsert(record);
            collection.EnsureIndex(x => x.ClubName);


            Application.Current.Post.ReplyMessage($"'{clubName}'に'{role.Name}'を設定したよ！", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void ClearRole(SocketMessage message, string[] arguments, PlayerGameData playerData)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("どの部活のロールを解除するの？", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var clubName = arguments[0];
            clubIDTable.Remove(clubName);


            using var database = new LiteDatabase(DatabaseName);
            var collection = database.GetCollection<RoleRecord>(CollectionName);
            var records = collection.Query().Where(x => x.ClubName == clubName);
        }



        private class RoleRecord
        {
            public string ClubName { get; set; }

            public ulong RoleID { get; set; }
        }
    }
}