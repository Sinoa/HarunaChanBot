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
using System.Text.RegularExpressions;
using HarunaChanBot.Framework;

namespace HarunaChanBot.Services
{
    public class AmazonUrlConvertService : ApplicationService
    {
        private static readonly HashSet<string> CommandHash = new HashSet<string>() { "AmazonURLを変換して", "アマゾンURLを変換して" };
        private static readonly Regex regex = new Regex("https?://[\\w!?/\\+\\-_~=;\\.,*&@#$%\\(\\)\'\\[\\]]+");



        protected internal override void Update()
        {
            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                if (!regex.IsMatch(message.Content))
                {
                    continue;
                }


                var url = default(Uri);
                try
                {
                    url = new Uri(message.Content);
                }
                catch (UriFormatException)
                {
                    continue;
                }


                if (url.Host != "www.amazon.co.jp")
                {
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
                    continue;
                }


                message.DeleteAsync();
                Application.Current.Post.ReplyMessage($"https://www.amazon.co.jp/dp/{productCode}", message);
            }
        }
    }
}