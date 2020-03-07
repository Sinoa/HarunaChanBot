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

using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordApplication
{
    public abstract class Application
    {
        private readonly DiscordSocketClient client;
        private readonly SynchronizationContext synchronizationContext;



        public Application()
        {
            client = new DiscordSocketClient();
            client.LoggedIn += Client_LoggedIn;
            client.LoggedOut += Client_LoggedOut;
            client.JoinedGuild += Client_JoinedGuild;
            client.LeftGuild += Client_LeftGuild;
            client.GuildAvailable += Client_GuildAvailable;
            client.MessageReceived += Client_MessageReceived;
        }


        private Task Client_LoggedIn()
        {
            synchronizationContext.Send(x => OnLoggedIn(), null);
            return Task.CompletedTask;
        }


        private Task Client_LoggedOut()
        {
            synchronizationContext.Send(x => OnLoggedOut(), null);
            return Task.CompletedTask;
        }


        private Task Client_JoinedGuild(SocketGuild arg)
        {
            throw new System.NotImplementedException();
        }


        private Task Client_LeftGuild(SocketGuild arg)
        {
            throw new System.NotImplementedException();
        }


        private Task Client_GuildAvailable(SocketGuild arg)
        {
            throw new System.NotImplementedException();
        }


        private Task Client_MessageReceived(SocketMessage arg)
        {
            throw new System.NotImplementedException();
        }


        protected virtual string GetBotToken()
        {
            return null;
        }


        protected virtual void OnLoggedIn()
        {
        }


        protected virtual void OnLoggedOut()
        {
        }
    }
}
