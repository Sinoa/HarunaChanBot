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
using System.Text.RegularExpressions;
using Discord.WebSocket;
using HarunaChanBot.Framework;
using MersenneTwister;

namespace HarunaChanBot.Services
{
    public class KotobaAsobiService : ApplicationService
    {
        private static readonly Regex MentionRegex = new Regex("<@!?([0-9]+)>");
        private static readonly char[] CharaArray = new char[] {
            'あ', 'い', 'う', 'え', 'お',
            'か', 'き', 'く', 'け', 'こ',
            'さ', 'し', 'す', 'せ', 'そ',
            'た', 'ち', 'つ', 'て', 'と',
            'な', 'に', 'ぬ', 'ね', 'の',
            'は', 'ひ', 'ふ', 'へ', 'ほ',
            'ま', 'み', 'む', 'め', 'も',
            'や', 'ゆ', 'よ',
            'ら', 'り', 'る', 'れ', 'ろ',
            'わ', 'を', 'ん',
            'が', 'ぎ', 'ぐ', 'げ', 'ご',
            'ざ', 'じ', 'ず', 'ぜ', 'ぞ',
            'だ', 'ぢ', 'づ', 'で', 'ど',
            'ば', 'び', 'ぶ', 'べ', 'ぼ',
            'ぱ', 'ぴ', 'ぷ', 'ぺ', 'ぽ'
        };

        private static readonly Random random = DsfmtRandom.Create();



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


            ApplicationMain.Current.Post.SendMessage($"{CharaArray[random.Next(0, CharaArray.Length)]}んこ", message.Channel);
        }


        private string RemoveMentionContent(string content)
        {
            return MentionRegex.Replace(content, string.Empty);
        }
    }
}