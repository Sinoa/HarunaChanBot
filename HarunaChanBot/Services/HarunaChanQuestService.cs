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
#else
            KaiwaParser.AddCommandHeaderText("陽菜ちゃんくえすと、");
            KaiwaParser.AddCommandHeaderText("はるなちゃんくえすと、");
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


            playerData.Exp += (int)(contentLength * channelRate);


            var currentLevel = playerData.Level;
            LevelUp(playerData);
            if (currentLevel < playerData.Level)
            {
                Application.Current.Post.ReplyMessage($"はるなちゃんクエストのプレイヤーレベルが {playerData.Level} に上がったよ！", message);
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


        private double GetChannelRate(SocketMessage message)
        {
            var table = gameData.ChannelRateTable;
            var id = message.Channel.Id;

            if (!table.ContainsKey(id))
            {
                return 1.0;
            }


            return gameData.ChannelRateTable[message.Channel.Id].Rate;
        }


        private PlayerGameData GetPlayerData(SocketMessage message)
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
                };


                table[ID] = data;
            }


            return data;
        }
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
    }



    public class HarunaGameData
    {
        public const int MAX_ENERGY = 1500;



        public int Level { get; }
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