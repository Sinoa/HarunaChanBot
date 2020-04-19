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
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;
using MersenneTwister;

namespace HarunaChanBot.Services
{
    public class HaipaiService : ApplicationService
    {
        protected internal override void Update()
        {
            var service = Application.Current.GetService<HarunaChanQuestService>();
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                if (message.Author.IsBot)
                {
                    continue;
                }


                if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments)) continue;
                if (command != "配牌して" && command != "はいぱいして") continue;

                var stampFlag = true;
                arguments ??= Array.Empty<string>();
                if (arguments.Length >= 1)
                {
                    if (arguments[0] == "文字列として")
                    {
                        stampFlag = false;
                    }
                }

                var random = DsfmtRandom.Create();
                var playerData = service.GetPlayerData(message);
                var deck = Shuffle(CreateDeck(), random);
                var haipai = TakePai(deck);
//                haipai = haipai.Select(x => (x & 0x40) != 0 ? (true, (byte)(x & 0x40)) : (false, x)).OrderBy(x => x.Item2).Select(x => (byte)(x.Item1 ? x.Item2 | 0x40 : x.Item2)).ToArray();
                Array.Sort(haipai);
                var buffer = new StringBuilder();
                foreach (var chara in haipai.Select(x => stampFlag ? ToMahjongStamp(x) : ToMahjongChara(x)))
                {
                    buffer.Append(chara);
                }


                var kyoku = random.Next(0, 2) == 0 ? "東" : "南";
                var number = random.Next(1, 5);
                var roll = random.Next(0, 2) == 0 ? "親" : "子";
                var doraShowID = deck[deck.Length - 5];
                var doraShowPai = stampFlag ? ToMahjongStamp(doraShowID) : ToMahjongChara(doraShowID);
                var tumoPaiID = TakeTumo(deck);
                var tumoPai = stampFlag ? ToMahjongStamp(tumoPaiID) : ToMahjongChara(tumoPaiID);
                Application.Current.Post.ReplyMessage($"今回の配牌はこれだよ！\n{kyoku}{number}局 {roll}\nドラ表示牌：{doraShowPai}\n{buffer}　ツモ：{tumoPai}", message, message.Channel, playerData.GetMentionSuffixText());
            }
        }


        private byte[] TakePai(byte[] deck)
        {
            var pai = new byte[4 * 3 + 1];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    pai[i * 4 + j] = deck[i * 12 + j];
                }
            }


            pai[12] = deck[3 * 12 + 4 + 1];
            return pai;
        }


        private byte TakeTumo(byte[] deck)
        {
            return deck[3 * 12 + 4 + 1 + 1];
        }


        private byte[] CreateDeck()
        {
            var deck = new byte[3 * 9 * 4 + 4 * 4 + 3 * 4];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        var cat = (i & 3) << 4 | ((j == 4 && k == 0) ? 0x40 : 0x00);
                        var num = (j + 1) & 0x0F;
                        deck[i * 36 + j * 4 + k] = (byte)(cat | num);
                    }
                }
            }


            var offset = 3 * 9 * 4;
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    var cat = 0x30;
                    deck[i * 4 + j + offset] = (byte)(cat | i);
                }
            }


            offset += 4 * 4;
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    var cat = 0x30;
                    var knd = 4 + i;
                    deck[i * 4 + j + offset] = (byte)(cat | knd);
                }
            }


            return deck;
        }


        private byte[] Shuffle(byte[] array, Random random)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                var selectedIndex = random.Next(0, array.Length - i);
                var tailIndex = array.Length - i - 1;
                var tmp = array[tailIndex];
                array[tailIndex] = array[selectedIndex];
                array[selectedIndex] = tmp;
            }


            return array;
        }


        private string ToMahjongChara(byte data)
        {
            data = (byte)(data & 0xBF);
            char highSurrogate = (char)0xD83C;
            if (0x01 <= data && data <= 0x09)
            {
                char lowSurrogate = (char)(0xDC07 + data - 1);
                return new string(new char[] { highSurrogate, lowSurrogate });
            }
            else if (0x10 <= data && data <= 0x19)
            {
                char lowSurrogate = (char)(0xDC19 + data - 0x10 - 1);
                return new string(new char[] { highSurrogate, lowSurrogate });
            }
            else if (0x20 <= data && data <= 0x29)
            {
                char lowSurrogate = (char)(0xDC10 + data - 0x20 - 1);
                return new string(new char[] { highSurrogate, lowSurrogate });
            }
            else if (0x30 <= data && data <= 0x33)
            {
                char lowSurrogate = (char)(0xDC00 + data - 0x30);
                return new string(new char[] { highSurrogate, lowSurrogate });
            }
            else if (0x34 == data)
            {
                char lowSurrogate = (char)(0xDC06);
                return new string(new char[] { highSurrogate, lowSurrogate });
            }
            else if (0x35 == data)
            {
                char lowSurrogate = (char)(0xDC05);
                return new string(new char[] { highSurrogate, lowSurrogate });
            }
            else if (0x36 == data)
            {
                char lowSurrogate = (char)(0xDC04);
                return new string(new char[] { highSurrogate, lowSurrogate });
            }
            else if (0x80 == data)
            {
                char lowSurrogate = (char)(0xDC2B);
                return new string(new char[] { highSurrogate, lowSurrogate });
            }


            return string.Empty;
        }


        private string ToMahjongStamp(byte data)
        {
            if (stampTable.TryGetValue(data, out var stamp)) return stamp;
            return string.Empty;
        }


        private Dictionary<byte, string> stampTable = new Dictionary<byte, string>()
        {
            { 0x01, "<:49_1M:667121355031969842>" },
            { 0x02, "<:48_2M:667121375240257556>" },
            { 0x03, "<:47_3M:667121390163329044>" },
            { 0x04, "<:46_4M:667121405195976715>" },
            { 0x05, "<:45_5M:667121422149222412>" },
            { 0x06, "<:44_6M:667121439807242253>" },
            { 0x07, "<:43_7M:667121456035135519>" },
            { 0x08, "<:42_8M:667121470635245620>" },
            { 0x09, "<:41_9M:667121484627443712>" },

            { 0x11, "<:29_1P:667121058372911108>" },
            { 0x12, "<:28_2P:667121073682120734>" },
            { 0x13, "<:27_3P:667121088504922112>" },
            { 0x14, "<:26_4P:667121102086209547>" },
            { 0x15, "<:25_5P:667121119605817344>" },
            { 0x16, "<:24_6P:667121136198483984>" },
            { 0x17, "<:23_7P:667121153202192405>" },
            { 0x18, "<:22_8P:667121168053960724>" },
            { 0x19, "<:21_9P:667121183195398162>" },

            { 0x21, "<:39_1S:667121204888469531>" },
            { 0x22, "<:38_2S:667121218997977096>" },
            { 0x23, "<:37_3S:667121235112624163>" },
            { 0x24, "<:36_4S:667121252078714890>" },
            { 0x25, "<:35_5S:667121273998016553>" },
            { 0x26, "<:34_6S:667121290452140042>" },
            { 0x27, "<:33_7S:667121305069289498>" },
            { 0x28, "<:32_8S:667121320991129640>" },
            { 0x29, "<:31_9S:667121338992820234>" },

            { 0x30, "<:57_1Z:667120916043530250>" },
            { 0x31, "<:56_2Z:667120937572892692>" },
            { 0x32, "<:55_3Z:667120949308686376>" },
            { 0x33, "<:54_4Z:667120961211990067>" },

            { 0x34, "<:53_5Z:667120979511607316>" },
            { 0x35, "<:52_6Z:667120999380025354>" },
            { 0x36, "<:51_7Z:667121017298223132>" },

            { 0x45, "<:63_0M:667121501576757274>" },
            { 0x55, "<:62_0P:667121513375465512>" },
            { 0x65, "<:61_0S:667121526872604682>" },
        };
    }
}