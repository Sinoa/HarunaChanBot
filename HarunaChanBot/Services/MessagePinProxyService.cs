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

using Discord.Rest;
using Discord.WebSocket;
using HarunaChanBot.Framework;

namespace HarunaChanBot.Services
{
    public class MessagePinProxyService : ApplicationService
    {
        private string pinStampName = "📌";



        protected internal override void Update()
        {
            if (ApplicationMain.Current.Reaction.Count < 1)
            {
                return;
            }


            foreach (var reaction in ApplicationMain.Current.Reaction)
            {
                PinMessageAsync(reaction);
            }
        }


        private async void PinMessageAsync(DiscordReaction reaction)
        {
            if (reaction.Reaction.Emote.Name != pinStampName)
            {
                return;
            }


            var message = (RestUserMessage)await reaction.TargetChannel.GetMessageAsync(reaction.MessageID);
            if (reaction.IsAdded)
            {
                await message.PinAsync();
            }
            else
            {
                await message.UnpinAsync();
            }
        }
    }
}