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
using Discord.WebSocket;

namespace HarunaChanBot.Framework
{
    public class DiscordManager : IDisposable
    {
        private bool disposed;
        private DiscordSocketClient discordClient;
        private object bufferLockObject;
        private Queue<DiscordMessage> backbufferQueue;
        private Queue<DiscordMessage> messageQueue;



        public DiscordManager()
        {
            bufferLockObject = new object();
            backbufferQueue = new Queue<DiscordMessage>(64);
            messageQueue = new Queue<DiscordMessage>(64);


            discordClient = new DiscordSocketClient();
            discordClient.LoggedIn += DiscordClient_LoggedIn;
            discordClient.LoggedOut += DiscordClient_LoggedOut;
            discordClient.MessageReceived += DiscordClient_MessageReceived;
            discordClient.GuildAvailable += DiscordClient_GuildAvailable;
        }


        ~DiscordManager()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }


            if (disposing)
            {
                discordClient.Dispose();
            }


            disposed = true;
        }


        private void EnqueueMessage(DiscordMessage message)
        {
            lock (bufferLockObject)
            {
                backbufferQueue.Enqueue(message);
            }
        }


        private void SwitchBuffer()
        {
            lock (bufferLockObject)
            {
                var temp = messageQueue;
                messageQueue = backbufferQueue;
                bufferLockObject = temp;
            }
        }


        private System.Threading.Tasks.Task DiscordClient_LoggedIn()
        {
            throw new NotImplementedException();
        }


        private System.Threading.Tasks.Task DiscordClient_LoggedOut()
        {
            throw new NotImplementedException();
        }


        private System.Threading.Tasks.Task DiscordClient_MessageReceived(SocketMessage arg)
        {
            throw new NotImplementedException();
        }


        private System.Threading.Tasks.Task DiscordClient_GuildAvailable(SocketGuild arg)
        {
            throw new NotImplementedException();
        }
    }
}