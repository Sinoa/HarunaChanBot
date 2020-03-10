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
using Discord.WebSocket;
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;
using MersenneTwister;
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
            var service = Application.Current.GetService<HarunaChanQuestService>();


            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                var playerData = service.GetPlayerData(message);


                if (!message.Author.IsBot)
                {
                    if (kaiwaData.IsTadaimaMatch(message.Content))
                    {
                        Application.Current.Post.ReplyMessage($"{kaiwaData.GetOkaeriMessage()}", message, message.Channel, playerData.GetMentionSuffixText());
                    }
                }


                if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments)) continue;

                switch (command)
                {
                    case "おやすみなさい":
                    case "おやすみ":
                    case "おやすみなんし":
                    case "お休みなさい":
                    case "おやすみなしあ":
                        Oyasumi(message, playerData);
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
                        Ohayo(message, playerData);
                        break;


                    case "そのまま言って":
                        Repeat(message, arguments, playerData);
                        break;


                    case "返事して":
                        Henzi(message, arguments, playerData);
                        break;


                    case "会話データをロードして":
                        DynamicLoadKaiwaData(message, playerData);
                        break;


                    case "ただいまリストの表示":
                        ShowTadaimaList(message, playerData);
                        break;


                    case "おかえりリストの表示":
                        ShowOkaeriList(message, playerData);
                        break;


                    case "ただいまの言葉を覚えて":
                        SetTadaimaMessage(message, playerData, arguments);
                        break;


                    case "おかえりの言葉を覚えて":
                        SetOkaeriMessage(message, playerData, arguments);
                        break;


                    case "ただいまの言葉を忘れて":
                        RemoveTadaimaMessage(message, playerData, arguments);
                        break;


                    case "おかえりの言葉を忘れて":
                        RemoveOkaeriMessage(message, playerData, arguments);
                        break;
                }
            }
        }


        private void ShowTadaimaList(SocketMessage message, PlayerGameData playerData)
        {
            if (kaiwaData.TadaimaPatterns == null || kaiwaData.TadaimaPatterns.Count == 0)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、ただいまリストが空っぽなんだ。。。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }

            var buffer = new StringBuilder();
            buffer.AppendLine("私が知ってるただいまの言葉リストだよ！");
            for (int i = 0; i < kaiwaData.TadaimaPatterns.Count; ++i)
            {
                buffer.AppendLine($"[{i.ToString().PadLeft(2)}]{kaiwaData.TadaimaPatterns[i]}");
            }


            Application.Current.Post.ReplyMessage(buffer.ToString(), message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void ShowOkaeriList(SocketMessage message, PlayerGameData playerData)
        {
            if (kaiwaData.OkaeriMessage == null || kaiwaData.OkaeriMessage.Count == 0)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、おかえりリストが空っぽなんだ。。。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }

            var buffer = new StringBuilder();
            buffer.AppendLine("私が知ってるおかえりの言葉リストだよ！");
            for (int i = 0; i < kaiwaData.OkaeriMessage.Count; ++i)
            {
                buffer.AppendLine($"[{i.ToString().PadLeft(2)}]{kaiwaData.OkaeriMessage[i]}");
            }


            Application.Current.Post.ReplyMessage(buffer.ToString(), message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void SetTadaimaMessage(SocketMessage message, PlayerGameData playerData, string[] arguments)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、陽菜、お母さんにお父さんからの言葉だけしか覚えちゃ駄目って言われてるの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (arguments == null || arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("何のただいまの言葉を覚えればいいの？", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var tadaima = arguments[0];
            kaiwaData.SetTadaimaPattern(tadaima);


            Application.Current.Post.ReplyMessage($"陽菜、'{tadaima}'がただいまの挨拶だっていうのを覚えたよ！", message, message.Channel, playerData.GetMentionSuffixText());
            SaveKaiwaData();
        }


        private void SetOkaeriMessage(SocketMessage message, PlayerGameData playerData, string[] arguments)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、陽菜、お母さんにお父さんからの言葉だけしか覚えちゃ駄目って言われてるの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (arguments == null || arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("何のおかえりの言葉を覚えればいいの？", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var tadaima = arguments[0];
            kaiwaData.SetOkaeriPattern(tadaima);


            Application.Current.Post.ReplyMessage($"陽菜、'{tadaima}'がおかえりの挨拶だっていうのを覚えたよ！", message, message.Channel, playerData.GetMentionSuffixText());
            SaveKaiwaData();
        }


        private void RemoveTadaimaMessage(SocketMessage message, PlayerGameData playerData, string[] arguments)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、陽菜、お母さんからお父さん以外の人からその指示を受けちゃ駄目って言われてるの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (arguments == null || arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("何のただいまの言葉を忘れればいいの？", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (!int.TryParse(arguments[0], out var index))
            {
                Application.Current.Post.ReplyMessage("うーん、教えてもらった文字が数字なのかわからないや", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (index < 0 || index >= kaiwaData.TadaimaPatterns.Count)
            {
                Application.Current.Post.ReplyMessage("教えてくれた番号に言葉がなかったよ？", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var tadaima = kaiwaData.TadaimaPatterns[index];
            kaiwaData.DeleteTadaimaPattern(index);
            SaveKaiwaData();
        }


        private void RemoveOkaeriMessage(SocketMessage message, PlayerGameData playerData, string[] arguments)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、陽菜、お母さんからお父さん以外の人からその指示を受けちゃ駄目って言われてるの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (arguments == null || arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("何のただいまの言葉を忘れればいいの？", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (!int.TryParse(arguments[0], out var index))
            {
                Application.Current.Post.ReplyMessage("うーん、教えてもらった文字が数字なのかわからないや", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            if (index < 0 || index >= kaiwaData.OkaeriMessage.Count)
            {
                Application.Current.Post.ReplyMessage("教えてくれた番号に言葉がなかったよ？", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var tadaima = kaiwaData.OkaeriMessage[index];
            kaiwaData.DeleteTadaimaPattern(index);
            SaveKaiwaData();
        }


        private void LoadKaiwaData()
        {
            var jsonData = File.ReadAllText("Assets/KaiwaData.json");
            kaiwaData = JsonConvert.DeserializeObject<KaiwaData>(jsonData);
        }


        private void SaveKaiwaData()
        {
            var jsonData = JsonConvert.SerializeObject(kaiwaData, Formatting.Indented);
            File.WriteAllText("Assets/KaiwaData.json", jsonData);
        }


        private void DynamicLoadKaiwaData(SocketMessage message, PlayerGameData playerData)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            LoadKaiwaData();
            var buffer = new StringBuilder();
            buffer.AppendLine($"会話データを読み込んだよ！");
            buffer.AppendLine($"おはよう会話が、{kaiwaData.OhayoMessages.Count}件");
            buffer.AppendLine($"おやすみ会話が、{kaiwaData.OyasumiMessages.Count}件");
            buffer.AppendLine($"おみくじデータが、{kaiwaData.OmikuziData.Count}件");
            buffer.AppendLine($"おかえりの言葉が、{kaiwaData.OkaeriMessage?.Count}件");
            buffer.AppendLine($"ただいまの言葉が、{kaiwaData.TadaimaPatterns?.Count}件");
            buffer.AppendLine($"あったよ！");
            Application.Current.Post.ReplyMessage(buffer.ToString(), message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void Oyasumi(SocketMessage message, PlayerGameData playerData)
        {
            Application.Current.Post.ReplyMessage($"{kaiwaData.GetOyasumiMessage()} また明日ね！", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void Ohayo(SocketMessage message, PlayerGameData playerData)
        {
            var omikuzi = kaiwaData.GetOmikuziData();
            Application.Current.Post.ReplyMessage($"{kaiwaData.GetOhayoMessage()} 今日の運勢は、、、{omikuzi.ResultMessage}だよ！{omikuzi.PostMessage}", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void Repeat(SocketMessage message, string[] arguments, PlayerGameData playerData)
        {
            if (arguments == null || arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("陽菜、なんて言えばいいの？", message, message.Channel, playerData.GetMentionSuffixText());
            }
            else
            {
                Application.Current.Post.SendMessage(string.Concat(arguments), message);
            }
        }


        private void Henzi(SocketMessage message, string[] arguments, PlayerGameData playerData)
        {
            if (arguments == null || arguments.Length < 1)
            {
                Application.Current.Post.ReplyMessage("陽菜、なんて返事をすればいいの？", message, message.Channel, playerData.GetMentionSuffixText());
            }
            else
            {
                Application.Current.Post.ReplyMessage($"うん、いいよ！『{string.Concat(arguments)}』", message, message.Channel, playerData.GetMentionSuffixText());
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
        private static readonly Random random = DsfmtRandom.Create();


        public List<string> OhayoMessages;
        public List<string> OyasumiMessages;
        public List<string> TadaimaPatterns;
        public List<string> OkaeriMessage;
        public List<OmikuziData> OmikuziData;



        public string GetOyasumiMessage()
        {
            return OyasumiMessages[random.Next(0, OyasumiMessages.Count)];
        }


        public string GetOhayoMessage()
        {
            return OhayoMessages[random.Next(0, OhayoMessages.Count)];
        }


        public bool IsTadaimaMatch(string message)
        {
            if (TadaimaPatterns == null || TadaimaPatterns.Count == 0)
            {
                return false;
            }


            foreach (var tadaima in TadaimaPatterns)
            {
                if (message.Contains(tadaima)) return true;
            }


            return false;
        }


        public void SetTadaimaPattern(string message)
        {
            if (TadaimaPatterns == null)
            {
                TadaimaPatterns = new List<string>();
            }


            TadaimaPatterns.Add(message);
        }


        public void SetOkaeriPattern(string message)
        {
            if (OkaeriMessage == null)
            {
                OkaeriMessage = new List<string>();
            }


            OkaeriMessage.Add(message);
        }


        public void DeleteTadaimaPattern(int index)
        {
            TadaimaPatterns?.RemoveAt(index);
        }


        public void DeleteOkaeriPattern(int index)
        {
            OkaeriMessage?.RemoveAt(index);
        }


        public string GetOkaeriMessage()
        {
            if (OkaeriMessage == null || OkaeriMessage.Count == 0)
            {
                return string.Empty;
            }


            return OkaeriMessage[random.Next(0, OkaeriMessage.Count)];
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