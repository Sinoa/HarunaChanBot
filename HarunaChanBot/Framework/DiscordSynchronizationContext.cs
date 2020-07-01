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
using System.Threading;

namespace HarunaChanBot.Framework
{
    internal sealed class DiscordSynchronizationContext : SynchronizationContext
    {
        private readonly int contextThreadID;
        private readonly Queue<Message> messageQueue;



        public DiscordSynchronizationContext(out Action messagePumpHandler)
        {
            AsyncOperationManager.SynchronizationContext = this;
            contextThreadID = Thread.CurrentThread.ManagedThreadId;
            messageQueue = new Queue<Message>();
            messagePumpHandler = DoProcessMessage;
        }


        public override void Send(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == contextThreadID)
            {
                d(state);
                return;
            }


            using var waitHandle = new ManualResetEvent(false);
            lock (messageQueue) messageQueue.Enqueue(new Message(d, state, waitHandle));
            waitHandle.WaitOne();
        }


        public override void Post(SendOrPostCallback d, object state)
        {
            lock (messageQueue) messageQueue.Enqueue(new Message(d, state, null));
        }


        private void DoProcessMessage()
        {
            lock (messageQueue)
            {
                var nowMessageCount = messageQueue.Count;
                for (int i = 0; i < nowMessageCount; ++i)
                {
                    messageQueue.Dequeue().Invoke();
                }
            }
        }



        private readonly struct Message
        {
            private readonly SendOrPostCallback callback;
            private readonly ManualResetEvent waitHandle;
            private readonly object state;



            public Message(SendOrPostCallback callback, object state, ManualResetEvent waitHandle)
            {
                this.callback = callback;
                this.waitHandle = waitHandle;
                this.state = state;
            }


            public void Invoke()
            {
                try
                {
                    callback(state);
                }
                catch
                {
                }
                finally
                {
                    waitHandle?.Set();
                }
            }
        }
    }
}