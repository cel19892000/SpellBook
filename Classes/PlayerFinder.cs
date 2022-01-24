using System;
using System.Net.Http;

namespace SpellBook
{
    public class PlayerFinder
    {
        public string url;
        public string uuid;

        public PlayerFinder(string Username)
        {
            url = GetPlayerUrl(Username);
            uuid = GetPlayerUUID(Username);
        }

        string GetPlayerUrl(string Username)
        {
            if (!GetPlayerUUID(Username).Equals("Unknown"))
                return "http://profile.knockturnmc.com/player/" + GetPlayerUUID(Username);
            else
                return "Unknown";
        }

        static string GetPlayerUUID(string Username)
        {
            DateTime utcDate = DateTime.UtcNow;
            int timespan = (Int32)utcDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string UUID = GetHttpContentAsString("https://api.mojang.com/users/profiles/minecraft/" + Username + "?at=" + timespan);
            int start = UUID.IndexOf("id\":") + 5;
            int end = UUID.IndexOf("\"}");
            try { return UUID[start..end]; }
            catch { return "Unknown"; }
        }

        static string GetHttpContentAsString(string Url)
        {
            HttpClient HttpClient = new HttpClient();
            HttpRequestMessage RequestMessage = new HttpRequestMessage(HttpMethod.Get, Url);
            return HttpClient.SendAsync(RequestMessage).Result.Content.ReadAsStringAsync().Result;
        }

    }

    
}
