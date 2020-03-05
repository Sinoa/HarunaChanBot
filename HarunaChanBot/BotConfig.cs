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

using System.IO;
using Newtonsoft.Json;

namespace HarunaChanBot
{
    internal class BotConfig
    {
        public const string DEFAULT_BOT_TOKEN_TEXT = "input here BotToken.";
        public const string DEFAULT_NOT_PERMITTED_MESSAGE = "Sorry, that command can only be executed by '%SV_NAME%'.";
        public const string DEFAULT_COMMAND_NOT_FOUND_MESSAGE = "command is not found '%CMD NAME%'.";
        public const string DEFAULT_SUPERVISOR_NAME = "";



        [JsonIgnore]
        public bool IsSetuped => BotToken != DEFAULT_BOT_TOKEN_TEXT;
        public string BotToken { get; set; }
        public string NotPermittedMessageTemplate { get; set; }
        public string CommandNotFoundMessageTemplate { get; set; }
        public string SupervisorName { get; set; }



        public static void CreateConfig(FileInfo configFileInfo)
        {
            var config = new BotConfig();
            config.BotToken = DEFAULT_BOT_TOKEN_TEXT;
            config.NotPermittedMessageTemplate = DEFAULT_NOT_PERMITTED_MESSAGE;
            config.CommandNotFoundMessageTemplate = DEFAULT_COMMAND_NOT_FOUND_MESSAGE;
            config.SupervisorName = DEFAULT_SUPERVISOR_NAME;


            var jsonData = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFileInfo.FullName, jsonData);
        }


        public static BotConfig LoadConfig(FileInfo configFileInfo)
        {
            if (!configFileInfo.Exists)
            {
                return null;
            }


            var jsonData = File.ReadAllText(configFileInfo.FullName);
            return JsonConvert.DeserializeObject<BotConfig>(jsonData);
        }


        public void SaveConfig(FileInfo configFileInfo)
        {
            var jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(configFileInfo.FullName, jsonData);
        }
    }
}
