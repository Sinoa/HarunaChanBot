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
using System.Threading.Tasks;

namespace HarunaChanBot.BotCommands
{
    public class ShowOyasumiCommand : BotCommand
    {
        private readonly string[] messages;



        public ShowOyasumiCommand()
        {
            messages = new string[]
            {
                "おやすみなさい！",
                "おやすみーとそーす！",
                "おやすみずーり！",
                "おやすみっどうぇい！",
                "おやすみらくるにき！",
                "おやすみそしる！",
            };
        }


        public override Task RunCommand(BotCommandContext context)
        {
            var random = new Random();
            var selectedMessage = messages[random.Next(0, messages.Length)];
            return context.BotClient.ReplyMessage($"{selectedMessage} また明日ね！", context);
        }
    }
}
