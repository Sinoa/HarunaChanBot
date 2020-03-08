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

using System.Collections.Generic;

namespace HarunaChanBot.Utils
{
    public static class KaiwaParser
    {
#if DEBUG
        private static readonly string[] CommandHeaderTexts = new string[] { "のあちゃん、" };
#else
        private static readonly string[] CommandHeaderTexts = new string[] { "陽菜ちゃん、", "お願い陽菜ちゃん、", "はるなちゃん、", "おねがいはるなちゃん、", "!HB:" };
#endif



        public static bool ParseMessageCommand(string message, out string command, out string[] arguments)
        {
            command = null;
            arguments = null;
            foreach (var commandHeaderText in CommandHeaderTexts)
            {
                if (message.StartsWith(commandHeaderText))
                {
                    var splitedMessage = SplitMessage(message.Remove(0, commandHeaderText.Length));
                    command = splitedMessage[0];
                    if (splitedMessage.Length > 1)
                    {
                        arguments = new string[splitedMessage.Length - 1];
                        for (int i = 1; i < splitedMessage.Length; ++i)
                        {
                            arguments[i - 1] = splitedMessage[i];
                        }
                    }
                    break;
                }
            }


            return command != null;
        }


        private static string[] SplitMessage(string message)
        {
            var stringList = new List<string>();
            var charaList = new List<char>();
            foreach (var chara in message)
            {
                if (char.IsWhiteSpace(chara))
                {
                    stringList.Add(new string(charaList.ToArray()));
                    charaList.Clear();
                    continue;
                }


                charaList.Add(chara);
            }


            if (stringList.Count == 0)
            {
                return new string[] { message };
            }


            if (charaList.Count != 0)
            {
                stringList.Add(new string(charaList.ToArray()));
            }


            return stringList.ToArray();
        }
    }
}
