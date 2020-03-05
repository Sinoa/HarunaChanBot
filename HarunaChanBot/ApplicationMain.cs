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
using System.Threading.Tasks;

namespace HarunaChanBot
{
    internal class ApplicationMain
    {
        private BotClient botClient;



        public static ApplicationContext Context { get; private set; }



        [STAThread]
        private static void Main()
        {
            new ApplicationMain().RunAsync().Wait();
        }


        private async Task RunAsync()
        {
            Console.WriteLine("Wakeup HarunaChanBot.");


            Context = new ApplicationContext();
            if (Context.Config == null)
            {
                Context.CreateConfig();
                Console.WriteLine("コンフィグファイルが見つからなかったため、新しく生成しました。");
                return;
            }


            if (!Context.Config.IsSetuped)
            {
                Console.WriteLine("コンフィグがまだ未設定です。コンフィグの設定を完了してください。");
                return;
            }


            botClient = new BotClient();
            var returnCode = await botClient.DoRunLoop(Context.Config.BotToken);


            Console.WriteLine("Exited run loop.");
            Context.SaveContext();
            Console.WriteLine("Completed save context.");


            Environment.ExitCode = returnCode;
        }
    }
}
