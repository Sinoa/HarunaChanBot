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

using HarunaChanBot.Framework;
using HarunaChanBot.Utils;

namespace HarunaChanBot.Services
{
    public class SystemControlService : ApplicationService
    {
        protected internal override void Update()
        {
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments)) continue;


                switch (command)
                {
                    case "家に帰って":
                        if (message.Author.Id != Application.Current.SupervisorID)
                        {
                            Application.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message);
                            return;
                        }


                        Application.Current.Post.SendMessage("はーい！陽菜、お家に帰るね～、ばいばーい", message);
                        Application.Current.Quit();
                        break;
                }
            }
        }
    }
}