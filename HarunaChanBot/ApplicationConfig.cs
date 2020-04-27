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
    public class ApplicationConfig
    {
        private FileInfo configFileInfo;

        public string DiscordBotToken { get; set; }



        public ApplicationConfig(FileInfo configFileInfo)
        {
            this.configFileInfo = configFileInfo;
        }


        public void Load()
        {
            configFileInfo.Refresh();
            if (!configFileInfo.Exists)
            {
                DiscordBotToken = null;
                Save();
                return;
            }


            var jsonData = File.ReadAllText(configFileInfo.FullName);
            var loadedObject = JsonConvert.DeserializeObject<ApplicationConfig>(jsonData);
            DiscordBotToken = loadedObject.DiscordBotToken;
        }


        public void Save()
        {
            var jsonData = JsonConvert.SerializeObject(this);
            File.WriteAllText(configFileInfo.FullName, jsonData);
        }
    }
}