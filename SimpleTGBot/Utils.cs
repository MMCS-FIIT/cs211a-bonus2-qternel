using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Text.RegularExpressions;
namespace Utils
{
    class Utils
    {

        public static async void GetData(string handle)
        {

            await Task.Run(() =>
            {
                string url = $"https://codeforces.com/api/user.info?handles={handle}";
                var client = new RestClient(url);
                var request = new RestRequest();

                var response = client.Get(request);

                Console.WriteLine(response.Content.ToString());
                string userFile = $"userData_{handle}.txt";
                if (System.IO.File.Exists(userFile))
                    return;

                System.IO.File.WriteAllTextAsync(userFile, response.Content.ToString());

            });

        }

    }
}
