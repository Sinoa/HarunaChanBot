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
using System.Runtime;
using Discord.WebSocket;
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;

namespace HarunaChanBot.Services
{
    public class SystemControlService : ApplicationService
    {
        private double maxFrameNanoTime;



        protected internal override void Update()
        {
            maxFrameNanoTime = Math.Max(maxFrameNanoTime, Application.Current.FrameNanoTime);
            var service = Application.Current.GetService<HarunaChanQuestService>();


            foreach (var message in Application.Current.Post.ReceivedMessageList)
            {
                if (!KaiwaParser.ParseMessageCommand(message.Content, out var command, out var arguments)) continue;

                var playerData = service.GetPlayerData(message);


                switch (command)
                {
                    case "システムステータスの表示":
                        ShowSystemInformation(message);
                        break;


                    case "メモリ掃除をお願い":
                        GCCollect(message, playerData);
                        break;


                    case "家に帰って":
                        Logout(message, playerData);
                        break;
                }
            }
        }


        private void GCCollect(SocketMessage message, PlayerGameData playerData)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var prevSize = GC.GetTotalMemory(false);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            var nextSize = GC.GetTotalMemory(false);


            Application.Current.Post.ReplyMessage($"メモリの掃除が終わったよ！ 使用量は {prevSize.ToString("N0")} -> {nextSize.ToString("N0")} になったよ", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void ShowSystemInformation(SocketMessage message)
        {
            var powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;
            var messageText = $@"
現在稼働中のシステムのステータスを開示します。
フレーム処理時間：{(Application.Current.FrameNanoTime / 1000000.0).ToString("N3")} ms（{(1.0 / Application.Current.FrameNanoTime * 1000000000.0).ToString("N0")} FPS）
最大フレーム処理時間：{(maxFrameNanoTime / 1000000.0).ToString("N3")} ms（{(1.0 / maxFrameNanoTime * 1000000000.0).ToString("N0")} FPS）
メモリ使用量：{Environment.WorkingSet.ToString("N0")} Bytes
GC使用量：{GC.GetTotalMemory(false).ToString("N0")} Bytes
GC世代数：{GC.MaxGeneration + 1} 世代
GCカウント(世代0)：{GC.CollectionCount(0)} 回
GCカウント(世代1)：{GC.CollectionCount(1)} 回
GCカウント(世代2)：{GC.CollectionCount(2)} 回
電源接続状況：{powerStatus.PowerLineStatus}
電池充電状態：{powerStatus.BatteryChargeStatus}
電池充電割合：{(int)(powerStatus.BatteryLifePercent * 100)} %";
            Application.Current.Post.ReplyMessage(messageText, message);
        }


        private void Logout(SocketMessage message, PlayerGameData playerData)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            Application.Current.Post.SendMessage("はーい！陽菜、お家に帰るね～、ばいばーい", message);
            Application.Current.Quit();
        }
    }
}