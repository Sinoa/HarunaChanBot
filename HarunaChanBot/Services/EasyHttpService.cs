﻿// zlib/libpng License
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
using System.Linq;
using System.Net;
using System.Text;
using Discord.WebSocket;
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;
using Org.BouncyCastle.Asn1.Ocsp;

namespace HarunaChanBot.Services
{
    public class EasyHttpService : ApplicationService
    {
        private readonly HttpListener httpServer;
        private readonly Dictionary<ulong, byte[]> pageCache;
        private readonly string hostAddress;
        private bool exit;



        public EasyHttpService()
        {
            hostAddress = Environment.GetEnvironmentVariable("HOST_ADDR");


            pageCache = new Dictionary<ulong, byte[]>();
            pageCache[0] = new UTF8Encoding(false).GetBytes("Require ChannelID query parameter.");


            httpServer = new HttpListener();
            httpServer.Prefixes.Add("http://*/");
            httpServer.Start();
            DoHttpServerProcess();
        }


        private async void DoHttpServerProcess()
        {
            while (!exit)
            {
                try
                {
                    var context = await httpServer.GetContextAsync();
                    var channelIDParam = context.Request.QueryString.Get("ChannelID") ?? "0";
                    if (!ulong.TryParse(channelIDParam, out var channelID))
                    {
                        channelID = 0;
                    }


                    if (!pageCache.TryGetValue(channelID, out var responseData))
                    {
                        responseData = CachePage(channelID);
                    }


                    var response = context.Response;
                    response.StatusCode = 200;
                    response.OutputStream.Write(responseData);
                    response.Close();
                }
                catch
                {
                    break;
                }
            }
        }


        protected internal override void Terminate()
        {
            httpServer.Stop();
            httpServer.Close();
        }


        protected internal override void Update()
        {
            var service = Application.Current.GetService<HarunaChanQuestService>();


            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments)) continue;

                var playerData = service.GetPlayerData(message);


                switch (command)
                {
                    case "ファイルリストURLを教えて":
                        ShowPageUrl(message, arguments, playerData);
                        break;
                }
            }
        }



        private void ShowPageUrl(SocketMessage message, string[] arguments, PlayerGameData playerData)
        {
            pageCache[message.Channel.Id] = CachePage(message.Channel.Id);
            Application.Current.Post.ReplyMessage($"http://{hostAddress}/?ChannelID={message.Channel.Id}", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private byte[] CachePage(ulong channelID)
        {
            using var database = new DatabaseContext();
            var result = database.MessageAttachement
                .Where(x => x.ChannelID == channelID)
                .OrderByDescending(x => x.ID)
                .Take(100);


            var buffer = new StringBuilder();
            if (result.Count() == 0)
            {
                buffer.Append("No Data");
            }
            else
            {
                foreach (var record in result)
                {
                    buffer.Append($"<a href=\"{record.AttachmentURL}\">{record.FileName}</a><br/><img src=\"{record.AttachmentURL}\" width=\"25%\"/><br/><br/>\n");
                }
            }


            return new UTF8Encoding(false).GetBytes(buffer.ToString());
        }
    }
}