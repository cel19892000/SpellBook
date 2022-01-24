namespace SpellBook
{
    public class PlayerData
    {
        readonly SpellProgress sp = new SpellProgress();

        public PlayerData() { }

        private string _name = string.Empty;

        public string Name 
        { 
            get { return MainWindow.doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[1]/h2/text()").InnerText; }
            set { _name = value; }
        }

        public string Year 
        { 
            get { return MainWindow.doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[2]/div[3]/div").InnerText; }
            set { _name = value; }
        }
        public string House 
        { 
            get { return MainWindow.doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[2]/div[2]/div").InnerText; }
            set { _name = value; }
        }
        public string HousePoints 
        { 
            get { return MainWindow.doc.DocumentNode.SelectSingleNode("//*[@id=\"user-data\"]/tbody/tr[4]/td").InnerText; }
            set { _name = value; }
        }
        public string PlayerImageSource 
        { 
            get { return sp.SearchAttribute(MainWindow.doc, "//*[@class=\"four wide column center aligned\"]/img", "src"); }
            set { _name = value; }
        }
    }
}
