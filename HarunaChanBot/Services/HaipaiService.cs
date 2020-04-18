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


                var playerData = service.GetPlayerData(message);
                var deck = Shuffle(CreateDeck(), DsfmtRandom.Create());
                var haipai = TakePai(deck);
                var buffer = new StringBuilder();
                foreach (var chara in haipai.Select(x => ToMahjongChara(x)))
                {
                    buffer.Append(chara);
                }


                var twitterText = Uri.EscapeUriString($"今回の配牌\n{buffer}\n\n#麻雀\n#配牌");
                Application.Current.Post.ReplyMessage($"今回の配牌はこれだよ！\n{buffer}\nTwitter投稿はこちら：https://twitter.com/intent/tweet?text={twitterText}", message, message.Channel, playerData.GetMentionSuffixText());
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


        private byte[] CreateDeck()
        {
            var deck = new byte[3 * 9 * 4 + 4 * 4 + 3 * 4];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        var cat = (i & 3) << 4;
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


            return string.Empty;
        }
    }
}