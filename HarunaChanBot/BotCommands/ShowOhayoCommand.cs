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
using System.Linq;
using System.Threading.Tasks;

namespace HarunaChanBot.BotCommands
{
    public class ShowOhayoCommand : BotCommand
    {
        private readonly string[] messages;
        private readonly OmikuziData[] omikuzis;



        public ShowOhayoCommand()
        {
            omikuzis = new OmikuziData[]
            {
                new OmikuziData(2, "おぉ！天和モード！", "役満とか出せちゃうかも！？"),
                new OmikuziData(5, "大吉", "今日はガンガン全ツしていこう！"),
                new OmikuziData(10, "中吉", "イケイケだね！"),
                new OmikuziData(50, "小吉", "麻雀日和だね！"),
                new OmikuziData(100, "吉", "今日も麻雀を楽しく遊ぼう！"),
                new OmikuziData(8, "末吉", "押し引きに注意しながら打とう！"),
                new OmikuziData(3, "凶", "うーん、今日は降りを優先したほうがいいかも？"),
                new OmikuziData(2, "大凶", "今日は段位戦に注意してね。。。！"),
                new OmikuziData(1, "あちゃぁ...地獄モード", "役満に振り込んじゃうかもしれないからお休みしようよ！"),
            };


            messages = new string[]
            {
                "おはよー！",
                "おはYostar！！",
                "おはるな！！",
                "おはよんま！",
                "おはよるのずく！",
            };
        }


        public override Task RunCommand(BotCommandContext context)
        {
            var random = new Random();
            var selectedMessage = messages[random.Next(0, messages.Length)];
            var selectedOmikuzi = SelectOmikuzi(random);
            return context.BotClient.ReplyMessage($"{selectedMessage} 今日の運勢は、、、{selectedOmikuzi.ResultMessage}だよ！{selectedOmikuzi.PostMessage}", context);
        }



        private OmikuziData SelectOmikuzi(Random random)
        {
            var maxWeight = omikuzis.Sum(x => x.Weight);
            var value = random.Next(0, maxWeight);
            foreach (var omikuzi in omikuzis)
            {
                value -= omikuzi.Weight;
                if (value <= 0)
                {
                    return omikuzi;
                }
            }


            return omikuzis[0];
        }



        private struct OmikuziData
        {
            public int Weight;
            public string ResultMessage;
            public string PostMessage;



            public OmikuziData(int weight, string resultMessage, string postMessage)
            {
                Weight = weight;
                ResultMessage = resultMessage;
                PostMessage = postMessage;
            }
        }
    }
}
