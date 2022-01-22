using HtmlAgilityPack;

namespace SpellBook
{
    public class PlayerData
    {
        readonly SpellProgress sp = new SpellProgress();
        
        public string name;
        public string year;
        public string house;
        public string housePoints;
        public string playerImageSource;

        public PlayerData() { }

        public PlayerData(HtmlDocument doc)
        {
            name = doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[1]/h2/text()").InnerText;
            year = doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[2]/div[3]/div").InnerText;
            house = doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[2]/div[2]/div").InnerText;
            housePoints = doc.DocumentNode.SelectSingleNode("//*[@id=\"user-data\"]/tbody/tr[4]/td").InnerText;
            playerImageSource = sp.SearchAttribute(doc, "//*[@class=\"four wide column center aligned\"]/img", "src");
        }
    }
}
