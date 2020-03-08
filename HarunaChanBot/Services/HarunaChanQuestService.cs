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
using System.IO;
using System.Text;
using Discord.WebSocket;
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;
using Newtonsoft.Json;

namespace HarunaChanBot.Services
{
    public class HarunaChanQuestService : ApplicationService
    {
        private GameData gameData;
        private Stopwatch stopwatch;
        private bool dirty;
        private Dictionary<int, int> expTable;



        public HarunaChanQuestService()
        {
#if DEBUG
            KaiwaParser.AddCommandHeaderText("のあちゃんくえすと、");
            KaiwaParser.AddCommandHeaderText("のあちゃんクエスト、");
#else
            KaiwaParser.AddCommandHeaderText("陽菜ちゃんくえすと、");
            KaiwaParser.AddCommandHeaderText("はるなちゃんくえすと、");
            KaiwaParser.AddCommandHeaderText("陽菜ちゃんクエスト、");
            KaiwaParser.AddCommandHeaderText("はるなちゃんクエスト、");
#endif


            expTable = new Dictionary<int, int>()
            {
                { 1, 10000 },
                { 2, 20000 },
                { 3, 30000 },
                { 4, 40000 },
                { 5, 50000 },
                { 6, 60000 },
                { 7, 70000 },
                { 8, 80000 },
                { 9, 90000 },
                { 10, 100000 },
                { 11, 110000 },
                { 12, 120000 },
                { 13, 130000 },
                { 14, 140000 },
                { 15, 150000 },
                { 16, 160000 },
                { 17, 170000 },
                { 18, 180000 },
                { 19, 190000 },
                { 20, int.MaxValue },
            };


            if (!Directory.Exists("SaveData"))
            {
                Directory.CreateDirectory("SaveData");
            }


            Load();


            stopwatch = Stopwatch.StartNew();
        }


        protected internal override void Update()
        {
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                ProcessGame(message);
            }


            if (stopwatch.ElapsedMilliseconds >= 30 * 1000 && dirty)
            {
                dirty = false;
                Save();
            }
        }


        protected internal override void Terminate()
        {
            Save();
        }


        private void Save()
        {
            var jsonData = JsonConvert.SerializeObject(gameData);
            File.WriteAllText("SaveData/harunachanquest.json", jsonData);
        }


        private void Load()
        {
            if (!File.Exists("SaveData/harunachanquest.json"))
            {
                gameData = new GameData();
                gameData.HarunaChan = new HarunaGameData()
                {
                    Level = 1,
                    Exp = 0,
                    Energy = HarunaGameData.MAX_ENERGY,
                };
                gameData.PlayerTable = new Dictionary<ulong, PlayerGameData>();
                gameData.ChannelRateTable = new Dictionary<ulong, ChannelRate>();
                return;
            }


            var jsonData = File.ReadAllText("SaveData/harunachanquest.json");
            gameData = JsonConvert.DeserializeObject<GameData>(jsonData);
        }


        private void ProcessGame(SocketMessage message)
        {
            if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments))
            {
                AddDailyCoin(message);
                AddExp(message);
                return;
            }


            switch (command)
            {
                case "ステータス確認":
                case "ステータス表示":
                case "ステータスの確認":
                case "ステータスの表示":
                    ShowPlayerStatus(message);
                    break;


                case "はるなちゃんの状態":
                case "陽菜ちゃんの状態":
                    ShowHarunaStatus(message);
                    break;


                case "チャンネルレート設定":
                case "チャンネルレートの設定":
                    SetChannelRate(message, arguments ?? Array.Empty<string>());
                    break;


                case "チャンネルレート確認":
                case "チャンネルレートの確認":
                    ShowChannelRate(message);
                    break;


                case "遊び方":
                    HowPlaying(message);
                    break;
            }
        }


        private void ShowPlayerStatus(SocketMessage message)
        {
            var playerData = GetPlayerData(message);
            Application.Current.Post.ReplyMessage($@"
レベル：{playerData.Level}
経験値：{playerData.Exp} / {expTable[playerData.Level]}
コイン：{playerData.Coin}
性別：{playerData.Gender}", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void ShowHarunaStatus(SocketMessage message)
        {
            var harunaData = gameData.HarunaChan;
            Application.Current.Post.ReplyMessage($@"
レベル：{harunaData.Level}
経験値：{harunaData.Exp} / {expTable[harunaData.Level]}
満腹度：{harunaData.Energy} / {HarunaGameData.MAX_ENERGY}", message);
        }


        private void SetChannelRate(SocketMessage message, string[] arguments)
        {
            var playerData = GetPlayerData(message);


            if (arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("コマンドの引数が足りないよぉ", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい。このコマンドはかんりしゃの人だけしか使っちゃいけないってお母さんに言われてるの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var channelRate = GetChannelRate(message);
            var currentRate = channelRate.Rate;
            channelRate.Rate = double.Parse(arguments[0]);
            Application.Current.Post.ReplyMessage($"{message.Channel.Name}({message.Channel.Id}) のレートを {currentRate} -> {channelRate.Rate} に変更したよ！", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void ShowChannelRate(SocketMessage message)
        {
            var playerData = GetPlayerData(message);
            Application.Current.Post.ReplyMessage($"{message.Channel.Name}({message.Channel.Id}) のレートは {GetChannelRate(message).Rate} だよ！", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void HowPlaying(SocketMessage message)
        {
            Application.Current.Post.ReplyMessage($@"
はるなちゃんクエストは、陽菜ちゃんbotが所属するディスコードサーバーへの発言をすると、プレイヤーが成長してさらに特定条件を満たすと、コインが貰えます。
もらったコインは陽菜ちゃんにアイスを買ってあげることで、陽菜ちゃんの満腹度が回復します。満腹度は時間とともに消費されて経験値になります。
増えた経験値は陽菜ちゃんの成長に繋がります。

コマンド一覧：
・ステータス確認
・ステータス表示
・ステータスの確認
・ステータスの表示
　現在のプレイヤー状態を確認できます。


・はるなちゃんの状態
・陽菜ちゃんの状態
　はるなちゃんのステータスを確認できます。


・チャンネルレート設定　引数１（管理者のみ）
・チャンネルレートの設定　引数１（管理者のみ）
　このコマンドを実行したチャンネルの経験値入手レートを設定します
　・引数１：レートを実数として入力


・チャンネルレート確認
・チャンネルレートの確認
　このコマンドを実行したチャンネルの現在の経験値入手レートを表示します


・遊び方
　この説明文を出してくれます。


攻略のヒント：
牌譜検証をしたり、麻雀部屋で麻雀の話をしているといいことあるかも？

注意：
コマンド発言は経験値取得対象外です。", message);
        }


        private void AddDailyCoin(SocketMessage message)
        {
            var playerData = GetPlayerData(message);
            var timestamp = message.Timestamp.ToOffset(TimeSpan.FromHours(9.0));
            if (playerData.LastSendMessageTimestamp.Day != timestamp.Day)
            {
                playerData.Coin += 100;
            }


            playerData.LastSendMessageTimestamp = timestamp;
        }


        private void AddExp(SocketMessage message)
        {
            var playerData = GetPlayerData(message);
            var channelRate = GetChannelRate(message);
            var contentLength = message.Content.Length;


            playerData.Exp += (int)(contentLength * channelRate.Rate);


            var currentLevel = playerData.Level;
            LevelUp(playerData);
            if (currentLevel < playerData.Level)
            {
                Application.Current.Post.ReplyMessage($"はるなちゃんクエストのプレイヤーレベルが {playerData.Level} に上がったよ！", message, message.Channel, playerData.GetMentionSuffixText());
            }

            dirty = true;
        }


        private void LevelUp(PlayerGameData playerData)
        {
            var nextExt = expTable[playerData.Level];
            while (playerData.Exp >= nextExt)
            {
                playerData.Exp -= nextExt;
                playerData.Level += 1;
                nextExt = expTable[playerData.Level];
            }
        }


        private ChannelRate GetChannelRate(SocketMessage message)
        {
            var table = gameData.ChannelRateTable;
            var id = message.Channel.Id;


            if (!table.TryGetValue(id, out var rate))
            {
                rate = new ChannelRate();
                rate.ID = id;
                rate.Rate = 1.0;
                table[id] = rate;
            }


            return rate;
        }


        public PlayerGameData GetPlayerData(SocketMessage message)
        {
            var table = gameData.PlayerTable;
            var ID = message.Author.Id;


            if (!table.TryGetValue(ID, out var data))
            {
                data = new PlayerGameData()
                {
                    LastSendMessageTimestamp = message.Timestamp.ToOffset(TimeSpan.FromHours(9.0)),
                    Level = 1,
                    Exp = 0,
                    Coin = 100,
                    Gender = GenderType.Male,
                };


                table[ID] = data;
            }


            return data;
        }
    }



    public enum GenderType : int
    {
        Male = 0,
        Female = 1,
    }



    public class ChannelRate
    {
        public ulong ID { get; set; }
        public double Rate { get; set; }
    }



    public class PlayerGameData
    {
        public int Level { get; set; }
        public int Exp { get; set; }
        public int Coin { get; set; }
        public DateTimeOffset LastSendMessageTimestamp { get; set; }
        public GenderType Gender { get; set; }



        public string GetMentionSuffixText()
        {
            switch (Gender)
            {
                case GenderType.Male: return "おにいちゃん！";
                case GenderType.Female: return "おねえちゃん！";
            }


            return string.Empty;
        }
    }



    public class HarunaGameData
    {
        public const int MAX_ENERGY = 1500;



        public int Level { get; set; }
        public int Exp { get; set; }
        public int Energy { get; set; }
    }



    public class GameData
    {
        public HarunaGameData HarunaChan { get; set; }
        public Dictionary<ulong, PlayerGameData> PlayerTable { get; set; }
        public Dictionary<ulong, ChannelRate> ChannelRateTable { get; set; }
    }
}