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
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using HarunaChanBot.Framework;
using HarunaChanBot.Utils;

namespace HarunaChanBot.Services
{
    public class DatabaseService : ApplicationService
    {
        private FirebaseAuthProvider provider;
        private FirebaseAuthLink authLink;
        private FirebaseClient database;



        protected internal override async Task Startup()
        {
            var apiKey = Environment.GetEnvironmentVariable("FB_API_KEY");
            var email = Environment.GetEnvironmentVariable("FB_EMAIL");
            var pass = Environment.GetEnvironmentVariable("FB_PASS");
            var dbUrl = Environment.GetEnvironmentVariable("FB_DB_URL");


            provider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            authLink = await provider.SignInWithEmailAndPasswordAsync(email, pass);
            database = new FirebaseClient(dbUrl, new FirebaseOptions() { AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken) });
        }


        protected internal override void Terminate()
        {
            database.Dispose();
            provider.Dispose();
            database = null;
            provider = null;
        }


        public ChildQuery CreateQuery(string path)
        {
            return database.Child(path);
        }
    }
}
