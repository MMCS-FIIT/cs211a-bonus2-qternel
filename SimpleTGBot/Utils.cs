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
using System.Net;

namespace Utils
{
    class Utils
    {
        public static async Task<String> GetData(string handle)
        {

            try
            {
                string url = $"https://codeforces.com/api/user.info?handles={handle}";
                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.GetAsync(request);

                Console.WriteLine(response.Content.ToString());


                return response.Content.ToString();
            }
            catch (Exception e)
            {
                return "Failed to find user";
            }


        }


        public static async Task<String> GetLastActiveBlogId()
        {

            try
            {

                string url = "https://codeforces.com/api/recentActions?maxCount=1";
                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.GetAsync(request);

                return Regex.Match(response.Content.ToString(), "\\\"id\\\":([0-9]+)").Groups[1].Value;
            }
            catch (Exception e)
            {
                return "Failed";
            }


        }



    }




}