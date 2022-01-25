using HtmlAgilityPack;
using System;
using System.Net.Http;

namespace SpellBook
{
    public class PlayerFinder
    {
        readonly SaveManager sm = new SaveManager();

        public string uuid;
        public string knockturnName = "";

        public PlayerFinder(string Username)
        {
            uuid = GetPlayerUUID(Username);

            if (!uuid.Equals("Unknown"))
            {
                HtmlDocument uuidDoc = sm.ImportSpellDataByUrl(GetPlayerUrl());
                HtmlNodeCollection childNodes = uuidDoc.DocumentNode.SelectSingleNode("//*[@class=\"ui container\"]").ChildNodes;
                foreach (HtmlNode node in childNodes) if (node.NodeType == HtmlNodeType.Element)
                {
                    switch (node.Name)
                    {
                        case "h2":
                            knockturnName = "Unknown";
                            return;
                        default:
                            knockturnName = uuidDoc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[1]/h2/text()").InnerText;
                            break;
                    }
                }
            }
        }

        public string GetPlayerUrl() => "http://profile.knockturnmc.com/player/" + uuid;

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
