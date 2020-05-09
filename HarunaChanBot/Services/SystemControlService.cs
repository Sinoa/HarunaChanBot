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
        private System.Windows.Forms.PowerLineStatus lastPowerLineStatus;
        private ISocketMessageChannel channel;
        private bool notifyed;



        public SystemControlService()
        {
            lastPowerLineStatus = System.Windows.Forms.SystemInformation.PowerStatus.PowerLineStatus;
        }


        protected internal override void Update()
        {
            maxFrameNanoTime = Math.Max(maxFrameNanoTime, ApplicationMain.Current.FrameNanoTime);
            var service = ApplicationMain.Current.GetService<HarunaChanQuestService>();


            foreach (var message in ApplicationMain.Current.Post.ReceivedMessageList)
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


                    case "システム通知はここにお願い":
                        SetChannel(message, playerData);
                        break;


                    case "家に帰って":
                        Logout(message, playerData);
                        break;
                }
            }


            UpdatePowerStatus();
        }


        private void UpdatePowerStatus()
        {
            if (channel == null)
            {
                return;
            }


            var currentPowerLineStatus = System.Windows.Forms.SystemInformation.PowerStatus.PowerLineStatus;
            if (lastPowerLineStatus != currentPowerLineStatus)
            {
                lastPowerLineStatus = currentPowerLineStatus;
                switch (lastPowerLineStatus)
                {
                    case System.Windows.Forms.PowerLineStatus.Unknown:
                    case System.Windows.Forms.PowerLineStatus.Online:
                        Application.Current.Post.SendMessage($"<@{Application.Current.SupervisorID}>ご飯が出来たよ！", channel);
                        break;


                    default:
                    case System.Windows.Forms.PowerLineStatus.Offline:
                        Application.Current.Post.SendMessage($"<@{Application.Current.SupervisorID}>ご飯がもう無いよ！", channel);
                        break;
                }


                notifyed = false;
            }


            if (currentPowerLineStatus == System.Windows.Forms.PowerLineStatus.Offline)
            {
                var currentPowerLevel = System.Windows.Forms.SystemInformation.PowerStatus.BatteryLifePercent;
                if (notifyed == false && currentPowerLevel <= 0.15f)
                {
                    Application.Current.Post.SendMessage($"<@{Application.Current.SupervisorID}>うぅ。。。お腹すいたぁ、陽菜ご飯を食べたいよ～", channel);
                    notifyed = true;
                }
            }
        }


        private void SetChannel(SocketMessage message, PlayerGameData playerData)
        {
            if (message.Author.Id != Application.Current.SupervisorID)
            {
                Application.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            channel = message.Channel;
            Application.Current.Post.ReplyMessage($"何かあったらここに連絡するね！", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void GCCollect(SocketMessage message, PlayerGameData playerData)
        {
            if (message.Author.Id != ApplicationMain.Current.SupervisorID)
            {
                ApplicationMain.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            var prevSize = GC.GetTotalMemory(false);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            var nextSize = GC.GetTotalMemory(false);


            ApplicationMain.Current.Post.ReplyMessage($"メモリの掃除が終わったよ！ 使用量は {prevSize.ToString("N0")} -> {nextSize.ToString("N0")} になったよ", message, message.Channel, playerData.GetMentionSuffixText());
        }


        private void ShowSystemInformation(SocketMessage message)
        {
            var powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;
            var remaining = TimeSpan.FromSeconds(powerStatus.BatteryLifeRemaining);
            var messageText = $@"
現在稼働中のシステムのステータスを開示します。
フレーム処理時間：{(ApplicationMain.Current.FrameNanoTime / 1000000.0).ToString("N3")} ms（{(1.0 / ApplicationMain.Current.FrameNanoTime * 1000000000.0).ToString("N0")} FPS）
最大フレーム処理時間：{(maxFrameNanoTime / 1000000.0).ToString("N3")} ms（{(1.0 / maxFrameNanoTime * 1000000000.0).ToString("N0")} FPS）
メモリ使用量：{Environment.WorkingSet.ToString("N0")} Bytes
GC使用量：{GC.GetTotalMemory(false).ToString("N0")} Bytes
GC世代数：{GC.MaxGeneration + 1} 世代
GCカウント(世代0)：{GC.CollectionCount(0)} 回
GCカウント(世代1)：{GC.CollectionCount(1)} 回
GCカウント(世代2)：{GC.CollectionCount(2)} 回
電源接続状況：{powerStatus.PowerLineStatus}
電池充電状態：{powerStatus.BatteryChargeStatus}
電池充電割合：{(int)(powerStatus.BatteryLifePercent * 100)} %
電池充電時間：{TimeSpan.FromSeconds(powerStatus.BatteryLifeRemaining)}";
            ApplicationMain.Current.Post.ReplyMessage(messageText, message);
        }


        private void Logout(SocketMessage message, PlayerGameData playerData)
        {
            if (message.Author.Id != ApplicationMain.Current.SupervisorID)
            {
                ApplicationMain.Current.Post.ReplyMessage("ごめんなさい、知らない人の言葉を信じちゃいけないってお母さんから言われているの。", message, message.Channel, playerData.GetMentionSuffixText());
                return;
            }


            ApplicationMain.Current.Post.SendMessage("はーい！陽菜、お家に帰るね～、ばいばーい", message);
            ApplicationMain.Current.Quit();
        }
    }
}