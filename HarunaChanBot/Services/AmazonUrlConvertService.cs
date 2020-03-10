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
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;

namespace HarunaChanBot.Services
{
    public class AmazonUrlConvertService : ApplicationService
    {
        private static readonly HashSet<string> CommandHash = new HashSet<string>() { "AmazonURLを変換して", "アマゾンURLを変換して" };



        protected internal override void Update()
        {
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments))
                {
                    continue;
                }


                if (!CommandHash.Contains(command))
                {
                    continue;
                }


                if (arguments != null && arguments.Length < 1)
                {
                    Application.Current.Post.ReplyMessage("どのAmazonURLを変換すればいいの？", message);
                    continue;
                }


                var url = default(Uri);
                try
                {
                    url = new Uri(arguments[0]);
                }
                catch (UriFormatException)
                {
                    Application.Current.Post.ReplyMessage("うーん、教えてもらったURLはちゃんとした形式じゃないみたいだよ", message);
                    continue;
                }


                if (url.Host != "www.amazon.co.jp")
                {
                    Application.Current.Post.ReplyMessage("もしかして、AmazonのURLじゃなかったりする？陽菜、わかるURLはAmazonだけだよ", message);
                    continue;
                }


                string productCode = null;
                foreach (var segment in url.Segments)
                {
                    if (segment == "gp/")
                    {
                        productCode = url.Segments[3].Replace("/", "");
                        break;
                    }


                    if (segment == "dp/")
                    {
                        if (url.Segments.Length == 3)
                        {
                            productCode = url.Segments[2];
                            break;
                        }


                        productCode = url.Segments[3].Replace("/", "");
                        break;
                    }
                }


                if (productCode == null)
                {
                    Application.Current.Post.ReplyMessage("ごめんなさい、商品コードを見つけられなかったから、変換出来なかったの。", message);
                    continue;
                }


                Application.Current.Post.ReplyMessage($"https://www.amazon.co.jp/dp/{productCode}", message);
            }
        }
    }
}