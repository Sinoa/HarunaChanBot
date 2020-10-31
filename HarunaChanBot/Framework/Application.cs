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
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace HarunaChanBot.Framework
{
    public abstract class Application<T> where T : Application<T>
    {
        private readonly DiscordSocketClient client;
        private readonly List<SocketMessage> receivedMessageList;
        private readonly List<DiscordMessageObject> transmissionMessageList;
        private readonly List<ApplicationService> serviceList;
        private readonly List<SocketGuild> guildList;
        private readonly List<DiscordReaction> reactionList;
        private SynchronizationContext synchronizationContext;



        public static T Current { get; private set; }


        public bool Running { get; private set; }


        public double FrameNanoTime { get; private set; }


        public DiscordMessagePost Post { get; }


        public IReadOnlyCollection<DiscordReaction> Reaction { get; }


        public ulong SupervisorID { get; private set; }



        public Application()
        {
            Current = (T)this;
            Startup();
            SupervisorID = GetSupervisorID();


            client = new DiscordSocketClient();
            client.LoggedIn += Client_LoggedIn;
            client.LoggedOut += Client_LoggedOut;
            client.JoinedGuild += Client_JoinedGuild;
            client.LeftGuild += Client_LeftGuild;
            client.GuildAvailable += Client_GuildAvailable;
            client.MessageReceived += Client_MessageReceived;
            client.ReactionAdded += Client_ReactionAdded;
            client.ReactionRemoved += Client_ReactionRemoved;


            receivedMessageList = new List<SocketMessage>();
            transmissionMessageList = new List<DiscordMessageObject>();
            guildList = new List<SocketGuild>();
            reactionList = new List<DiscordReaction>();
            Reaction = reactionList.AsReadOnly();
            Post = new DiscordMessagePost(receivedMessageList.AsReadOnly(), transmissionMessageList);


            serviceList = new List<ApplicationService>();
            InitializeService();
        }


        private Task Client_LoggedIn()
        {
            synchronizationContext.Post(x => OnLoggedIn_Core(), null);
            return Task.CompletedTask;
        }


        private Task Client_LoggedOut()
        {
            synchronizationContext.Post(x => OnLoggedOut_Core(), null);
            return Task.CompletedTask;
        }


        private Task Client_JoinedGuild(SocketGuild arg)
        {
            synchronizationContext.Post(x => OnJoinGuild_Core((SocketGuild)x), arg);
            return Task.CompletedTask;
        }


        private Task Client_LeftGuild(SocketGuild arg)
        {
            synchronizationContext.Post(x => OnLeaveGuild_Core((SocketGuild)x), arg);
            return Task.CompletedTask;
        }


        private Task Client_GuildAvailable(SocketGuild arg)
        {
            synchronizationContext.Post(x => OnGuildAvailable_Core((SocketGuild)x), arg);
            return Task.CompletedTask;
        }


        private Task Client_MessageReceived(SocketMessage arg)
        {
            synchronizationContext.Post(x => OnMessageReceived_Core((SocketMessage)x), arg);
            return Task.CompletedTask;
        }


        private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            synchronizationContext.Post(x => OnReactionAdded_Core((DiscordReaction)x), new DiscordReaction(arg2, arg1.Id, arg3, true));
            return Task.CompletedTask;
        }


        private Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            synchronizationContext.Post(x => OnReactionRemoved_Core((DiscordReaction)x), new DiscordReaction(arg2, arg1.Id, arg3, false));
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
            guildList.Add(guild);
            OnJoinGuild(guild);
        }


        private void OnLeaveGuild_Core(SocketGuild guild)
        {
            guildList.RemoveAll(x => x.Id == guild.Id);
            OnLeaveGuild(guild);
        }


        private void OnGuildAvailable_Core(SocketGuild guild)
        {
            guildList.Add(guild);
            OnGuildAvailable(guild);
            foreach (var service in serviceList)
            {
                service.OnGuildAvailable(guild);
            }
        }


        private void OnMessageReceived_Core(SocketMessage message)
        {
            receivedMessageList.Add(message);
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


        private void OnReactionAdded_Core(DiscordReaction reaction)
        {
            reactionList.Add(reaction);
        }


        private void OnReactionRemoved_Core(DiscordReaction reaction)
        {
            reactionList.Add(reaction);
        }


        protected virtual void InitializeService()
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


        protected virtual void Startup()
        {
        }


        protected virtual void Terminate()
        {
        }


        protected virtual string GetBotToken()
        {
            return null;
        }


        protected virtual ulong GetSupervisorID()
        {
            return 0;
        }


        public SocketGuild GetGuild(ulong id)
        {
            foreach (var guild in guildList)
            {
                if (guild.Id == id)
                {
                    return guild;
                }
            }


            return null;
        }


        public SocketTextChannel GetTextChannel(ulong id)
        {
            foreach (var guild in guildList)
            {
                foreach (var textChannel in guild.TextChannels)
                {
                    if (textChannel.Id == id)
                    {
                        return textChannel;
                    }
                }
            }


            return null;
        }


        protected void AddService(ApplicationService service)
        {
            if (serviceList.Contains(service)) return;
            serviceList.Add(service);
        }


        public TService GetService<TService>() where TService : ApplicationService
        {
            foreach (var service in serviceList)
            {
                if (service is TService) return (TService)service;
            }


            return null;
        }


        public void Quit()
        {
            synchronizationContext.Post(x => { Running = false; }, null);
        }


        public void Run()
        {
            AsyncOperationManager.SynchronizationContext = new DiscordSynchronizationContext(out var messagePumpHandler);
            synchronizationContext = AsyncOperationManager.SynchronizationContext;


            if (!StartupDiscord(messagePumpHandler))
            {
                OnStartupFailed_Core();
                return;
            }


            DoMainLoop(messagePumpHandler);
            ShutdownDiscord(messagePumpHandler);
            Terminate();
        }


        private void DoMainLoop(Action messagePumpHandler)
        {
            var stopwatch = new Stopwatch();


            Running = true;
            while (Running)
            {
                stopwatch.Restart();


                messagePumpHandler();
                Update_Core();
                UpdateService();
                receivedMessageList.Clear();
                reactionList.Clear();
                SendDiscordMessage(messagePumpHandler);
                Thread.Sleep(16);


                var tick = stopwatch.ElapsedTicks;
                FrameNanoTime = tick / (double)Stopwatch.Frequency * 1000000000.0;
            }


            TerminateService();
        }


        private void UpdateService()
        {
            foreach (var service in serviceList)
            {
                service.Update();
            }
        }


        private void TerminateService()
        {
            foreach (var service in serviceList)
            {
                service.Terminate();
            }
        }


        private void SendDiscordMessage(Action messagePumpHandler)
        {
            if (transmissionMessageList.Count == 0) return;


            var taskList = new List<Task<RestUserMessage>>();
            foreach (var messageObject in transmissionMessageList)
            {
                taskList.Add(messageObject.TargetChannel.SendMessageAsync(messageObject.Message));
            }


            var waitTask = Task.WhenAll(taskList);
            while (!waitTask.IsCompleted)
            {
                messagePumpHandler();
            }


            transmissionMessageList.Clear();
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