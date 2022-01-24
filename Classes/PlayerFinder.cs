using System;
using System.Net.Http;

namespace SpellBook
{
    public class PlayerFinder
    {
        public string url;

        public PlayerFinder(string Username)
        {
            url = GetPlayerUrl(Username);
        }

        string GetPlayerUrl(string Username)
        {
            return "http://profile.knockturnmc.com/player/" + GetPlayerUUID(Username);
        }

        static string GetPlayerUUID(string Username)
        {
            DateTime utcDate = DateTime.UtcNow;
            int timespan = (Int32)utcDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string UUID = GetHttpContentAsString("https://api.mojang.com/users/profiles/minecraft/" + Username + "?at=" + timespan);
            int start = UUID.IndexOf("id\":") + 5;
            int end = UUID.IndexOf("\"}");
            return UUID[start..end];
        }

        static string GetHttpContentAsString(string Url)
        {
            HttpClient HttpClient = new HttpClient();
            HttpRequestMessage RequestMessage = new HttpRequestMessage(HttpMethod.Get, Url);
            return HttpClient.SendAsync(RequestMessage).Result.Content.ReadAsStringAsync().Result;
        }

    }

    
}
