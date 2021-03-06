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

using System.Collections.Generic;
using Discord.WebSocket;

namespace HarunaChanBot.Framework
{
    public class DiscordMessagePost
    {
        private List<DiscordMessageObject> transmissionMessageList;



        public IReadOnlyList<SocketMessage> ReceivedMessageList;



        public DiscordMessagePost(IReadOnlyList<SocketMessage> receivedMessageList, List<DiscordMessageObject> transMissionMessageList)
        {
            ReceivedMessageList = receivedMessageList;
            this.transmissionMessageList = transMissionMessageList;
        }


        public void SendMessage(string message, SocketMessage socketMessage)
        {
            SendMessage(message, socketMessage.Channel);
        }


        public void SendMessage(string message, ISocketMessageChannel channel)
        {
            transmissionMessageList.Add(new DiscordMessageObject(channel, message));
        }


        public void ReplyMessage(string message, SocketMessage socketMessage)
        {
            ReplyMessage(message, socketMessage, socketMessage.Channel, string.Empty);
        }


        public void ReplyMessage(string message, SocketMessage socketMessage, ISocketMessageChannel channel)
        {
            ReplyMessage(message, socketMessage, channel, string.Empty);
        }


        public void ReplyMessage(string message, SocketMessage socketMessage, ISocketMessageChannel channel, string mentionSuffix)
        {
            transmissionMessageList.Add(new DiscordMessageObject(channel, $"{socketMessage.Author.Mention} {mentionSuffix} {message}"));
        }
    }
}
