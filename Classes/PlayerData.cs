namespace SpellBook
{
    public class PlayerData
    {
        //readonly SpellProgress sp = new SpellProgress();

        public PlayerData() { }

        private string _name = string.Empty;

        public string Name
        {
            get
            {
                if (MainWindow.data.KnockturnData.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[1]/h2/text()").InnerText != null)
                    return MainWindow.data.KnockturnData.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[1]/h2/text()").InnerText;
                return "Unknown";
            }
            set => _name = value;
        }

        public string Year
        {
            get => MainWindow.data.KnockturnData.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[2]/div[3]/div").InnerText;
            set => _name = value;
        }
        public string House
        {
            get => MainWindow.data.KnockturnData.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[2]/div[2]/div").InnerText;
            set => _name = value;
        }
        public string HousePoints
        {
            get => MainWindow.data.KnockturnData.DocumentNode.SelectSingleNode("//*[@id=\"user-data\"]/tbody/tr[4]/td").InnerText;
            set => _name = value;
        }
        public string PlayerImageSource
        {
            get => new SpellProgress().SearchAttribute(MainWindow.data.KnockturnData, "//*[@class=\"four wide column center aligned\"]/img", "src");
            set => _name = value;
        }
    }
}
