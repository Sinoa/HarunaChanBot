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

using System.Threading.Tasks;

namespace HarunaChanBot.BotCommands
{
    public class ShowUrls : BotCommand
    {
        public override Task RunCommand(BotCommandContext context)
        {
            return context.BotClient.SendMessage(
                @"陽菜、いろんなURLを知ってるよ！
雀愉レベル：****
雀愉ブログ：https://jantama.sinoa.ws/blog/
雀魂公式サイト：https://mahjongsoul.com/
雀魂公式ついったぁ（JP）：https://twitter.com/MahjongSoul_JP
雀魂公式ついったぁ（EN）：https://twitter.com/MahjongSoul_EN", context);
        }
    }
}
