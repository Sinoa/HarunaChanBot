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
using System.Text.RegularExpressions;
using Discord.WebSocket;
using HarunaChanBot.Framework;
using MersenneTwister;

namespace HarunaChanBot.Services
{
    public class SugoiwayoService : ApplicationService
    {
        private static readonly Regex MentionRegex = new Regex("<@!?([0-9]+)>");
        private static readonly string[] TilePattern = new string[] {
            "<:Xinia_Halloween_1L:749123315146424332>",
            "<:Xinia_Halloween_1R:749123315729301694>",
            "<:Xinia_Halloween_2L:749123315695616060>",
            "<:Xinia_Halloween_2R:749123315700072548>",
        };

        private readonly Random random;
        private SocketTextChannel timeSignalTargetChannel;
        private DateTimeOffset nextTimeSignalTime;



        public SugoiwayoService()
        {
            var tmp = DateTimeOffset.Now.AddHours(1.0);
            nextTimeSignalTime = new DateTimeOffset(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, 0, 0, TimeSpan.FromHours(9.0));
            random = DsfmtRandom.Create();
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
            }


            UpdateTimeSignal();
        }


        private void UpdateTimeSignal()
        {
            if (timeSignalTargetChannel == null || nextTimeSignalTime > DateTimeOffset.Now)
            {
                return;
            }


            CreateGaoo(timeSignalTargetChannel);


            nextTimeSignalTime = nextTimeSignalTime.AddHours(1.0);
            nextTimeSignalTime = new DateTimeOffset(
                nextTimeSignalTime.Year, nextTimeSignalTime.Month, nextTimeSignalTime.Day,
                nextTimeSignalTime.Hour, 30, 0,
                TimeSpan.FromHours(9.0));
        }


        private void Update(SocketMessage message)
        {
            var mentionRemovedContent = RemoveMentionContent(message.Content).Trim();
            var splitedContent = mentionRemovedContent.Replace("　", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = splitedContent[0];
            var arguments = new string[splitedContent.Length - 1];
            for (int i = 1; i < splitedContent.Length; ++i)
            {
                arguments[i - 1] = splitedContent[i];
            }


            ProcessCommand(message, command, arguments);
        }


        private string RemoveMentionContent(string content)
        {
            return MentionRegex.Replace(content, string.Empty);
        }


        private void ProcessCommand(SocketMessage message, string command, string[] arguments)
        {
            switch (command)
            {
                case "抽選はここでお願い": SetChannel(message, arguments); return;
                case "がおー": CreateGaoo(message.Channel); return;
            }
        }


        private void SetChannel(SocketMessage message, string[] arguments)
        {
            var isSupervisor = message.Author.Id == ApplicationMain.Current.Config.SupervisorUserID;
            var isSubSupervisor = ApplicationMain.Current.Config.SubSupervisorUserIDList.Contains(message.Author.Id);
            if (!(isSupervisor || isSubSupervisor))
            {
                ApplicationMain.Current.Post.SendMessage("only supervisor or sub supervisor can control this command.", message.Channel);
                return;
            }


            timeSignalTargetChannel = message.Channel as SocketTextChannel;
            ApplicationMain.Current.Post.SendMessage("Set completed...", timeSignalTargetChannel);
        }


        private void CreateGaoo(ISocketMessageChannel channel)
        {
            var result = Shuffle(new byte[] { 0, 1, 2, 3 }, random);
            var messageText = $"{TilePattern[result[0]]}{TilePattern[result[1]]}\n{TilePattern[result[2]]}{TilePattern[result[3]]}";
            ApplicationMain.Current.Post.SendMessage(messageText, channel);
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
    }
}