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
using System.IO;
using System.Linq;
using Discord.WebSocket;
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;
using Newtonsoft.Json;

namespace HarunaChanBot.Services
{
    public class KaiwaService : ApplicationService
    {
        private KaiwaData kaiwaData;



        public KaiwaService()
        {
            LoadKaiwaData();
        }


        protected internal override void Update()
        {
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments)) continue;


                switch (command)
                {
                    case "おやすみなさい":
                    case "おやすみ":
                    case "おやすみなんし":
                    case "お休みなさい":
                    case "おやすみなしあ":
                        Oyasumi(message);
                        break;


                    case "こんにちは":
                    case "こんにちわ":
                    case "こんちは":
                    case "こんちわ":
                        break;


                    case "おはよう":
                    case "おはよ":
                    case "おはようございます":
                    case "おはYostar":
                    case "おはYostar!":
                    case "おはYostar!!":
                    case "おはよーすたー":
                    case "おはよーすたー！":
                    case "おはよーすたー！！":
                        Ohayo(message);
                        break;


                    case "そのまま言って":
                        Repeat(message, arguments);
                        break;


                    case "返事して":
                        Henzi(message, arguments);
                        break;


                    case "会話データをロードして":
                        DynamicLoadKaiwaData(message);
                        break;
                }
            }
        }


        private void LoadKaiwaData()
        {
            var jsonData = File.ReadAllText("Assets/KaiwaData.json");
            kaiwaData = JsonConvert.DeserializeObject<KaiwaData>(jsonData);
        }


        private void DynamicLoadKaiwaData(SocketMessage message)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message);
                return;
            }


            LoadKaiwaData();
            Application.Current.Post.ReplyMessage($"会話データを読み込んだよ！\nおはよう会話が、{kaiwaData.OhayoMessages.Length}件\nおやすみ会話が、{kaiwaData.OyasumiMessages.Length}件\nおみくじデータが、{kaiwaData.OmikuziData.Length}件あったよ！", message);
        }


        private void Oyasumi(SocketMessage message)
        {
            Application.Current.Post.ReplyMessage($"{kaiwaData.GetOyasumiMessage()} また明日ね！", message);
        }


        private void Ohayo(SocketMessage message)
        {
            var omikuzi = kaiwaData.GetOmikuziData();
            Application.Current.Post.ReplyMessage($"{kaiwaData.GetOhayoMessage()} 今日の運勢は、、、{omikuzi.ResultMessage}だよ！{omikuzi.PostMessage}", message);
        }


        private void Repeat(SocketMessage message, string[] arguments)
        {
            if (arguments == null || arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("陽菜、なんて言えばいいの？", message);
            }
            else
            {
                Application.Current.Post.SendMessage(string.Concat(arguments), message);
            }
        }


        private void Henzi(SocketMessage message, string[] arguments)
        {
            if (arguments == null || arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("陽菜、なんて返事をすればいいの？", message);
            }
            else
            {
                Application.Current.Post.ReplyMessage($"うん、いいよ！『{string.Concat(arguments)}』", message);
            }
        }
    }



    public struct OmikuziData
    {
        public int Weight;
        public string ResultMessage;
        public string PostMessage;
    }



    public class KaiwaData
    {
        private Random random = new Random();


        public string[] OhayoMessages;
        public string[] OyasumiMessages;
        public OmikuziData[] OmikuziData;



        public string GetOyasumiMessage()
        {
            return OyasumiMessages[random.Next(0, OyasumiMessages.Length)];
        }


        public string GetOhayoMessage()
        {
            return OhayoMessages[random.Next(0, OhayoMessages.Length)];
        }


        public OmikuziData GetOmikuziData()
        {
            var maxWeight = OmikuziData.Sum(x => x.Weight);
            var value = random.Next(0, maxWeight);
            foreach (var omikuzi in OmikuziData.OrderByDescending(x => x.Weight))
            {
                value -= omikuzi.Weight;
                if (value <= 0)
                {
                    return omikuzi;
                }
            }


            return OmikuziData[0];
        }
    }
}