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
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace HarunaChanBot.Framework
{
    public abstract class Application
    {
        private readonly DiscordSocketClient client;



        public bool Running { get; private set; }


        public double FrameNanoTime { get; private set; }



        public Application()
        {
            client = new DiscordSocketClient();
            client.LoggedIn += Client_LoggedIn;
            client.LoggedOut += Client_LoggedOut;
            client.JoinedGuild += Client_JoinedGuild;
            client.LeftGuild += Client_LeftGuild;
            client.GuildAvailable += Client_GuildAvailable;
            client.MessageReceived += Client_MessageReceived;


            Initialize();
        }


        private Task Client_LoggedIn()
        {
            AsyncOperationManager.SynchronizationContext.Post(x => OnLoggedIn_Core(), null);
            return Task.CompletedTask;
        }


        private Task Client_LoggedOut()
        {
            AsyncOperationManager.SynchronizationContext.Post(x => OnLoggedOut_Core(), null);
            return Task.CompletedTask;
        }


        private Task Client_JoinedGuild(SocketGuild arg)
        {
            AsyncOperationManager.SynchronizationContext.Post(x => OnJoinGuild_Core((SocketGuild)x), arg);
            return Task.CompletedTask;
        }


        private Task Client_LeftGuild(SocketGuild arg)
        {
            AsyncOperationManager.SynchronizationContext.Post(x => OnLeaveGuild_Core((SocketGuild)x), arg);
            return Task.CompletedTask;
        }


        private Task Client_GuildAvailable(SocketGuild arg)
        {
            AsyncOperationManager.SynchronizationContext.Post(x => OnGuildAvailable_Core((SocketGuild)x), arg);
            return Task.CompletedTask;
        }


        private Task Client_MessageReceived(SocketMessage arg)
        {
            AsyncOperationManager.SynchronizationContext.Post(x => OnMessageReceived_Core((SocketMessage)x), arg);
            return Task.CompletedTask;
        }


        private void OnLoggedIn_Core()
        {
            OnLoggedIn();
        }


        private void OnLoggedOut_Core()
        {
            OnLoggedOut();
        }


        private void OnJoinGuild_Core(SocketGuild guild)
        {
            OnJoinGuild(guild);
        }


        private void OnLeaveGuild_Core(SocketGuild guild)
        {
            OnLeaveGuild(guild);
        }


        private void OnGuildAvailable_Core(SocketGuild guild)
        {
            OnGuildAvailable(guild);
        }


        private void OnMessageReceived_Core(SocketMessage message)
        {
            OnMessageReceived(message);
        }


        private void Update_Core()
        {
            Update();
        }


        private void OnStartupFailed_Core()
        {
            OnStartupFailed();
        }


        protected virtual void Initialize()
        {
        }


        protected virtual void OnLoggedIn()
        {
        }


        protected virtual void OnLoggedOut()
        {
        }


        protected virtual void OnJoinGuild(SocketGuild guild)
        {
        }


        protected virtual void OnLeaveGuild(SocketGuild guild)
        {
        }


        protected virtual void OnGuildAvailable(SocketGuild guild)
        {
        }


        protected virtual void OnMessageReceived(SocketMessage message)
        {
        }


        protected virtual void Update()
        {
        }


        protected virtual void OnStartupFailed()
        {
        }


        protected virtual string GetBotToken()
        {
            return null;
        }


        public void Run()
        {
            AsyncOperationManager.SynchronizationContext = new DiscordSynchronizationContext(out var messagePumpHandler);


            if (!StartupDiscord(messagePumpHandler))
            {
                OnStartupFailed_Core();
                return;
            }


            DoMainLoop(messagePumpHandler);
            ShutdownDiscord(messagePumpHandler);
        }


        private void DoMainLoop(Action messagePumpHandler)
        {
            var stopwatch = new Stopwatch();
            var spinwait = new SpinWait();


            Running = true;
            while (Running)
            {
                stopwatch.Restart();


                messagePumpHandler();
                Update_Core();
                spinwait.SpinOnce();


                var tick = stopwatch.ElapsedTicks;
                FrameNanoTime = tick / (double)Stopwatch.Frequency * 1000000000.0;
            }
        }


        private bool StartupDiscord(Action messagePumpHandler)
        {
            var task = StartupDiscordAsync();
            while (!task.IsCompleted)
            {
                messagePumpHandler();
            }


            return task.Result;
        }


        private async Task<bool> StartupDiscordAsync()
        {
            var token = GetBotToken();
            if (token == null) return false;


            try
            {
                await client.LoginAsync(Discord.TokenType.Bot, token);
                await client.StartAsync();
            }
            catch
            {
                return false;
            }


            return true;
        }


        private void ShutdownDiscord(Action messagePumpHandler)
        {
            var task = ShutdownDiscordAsync();
            while (!task.IsCompleted)
            {
                messagePumpHandler();
            }
        }


        private async Task ShutdownDiscordAsync()
        {
            await client.LogoutAsync();
        }
    }
}