using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using System.IO;

namespace SpellBook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// To Do List
    /// Spell Entry Graphics
    /// Edit Spell Button Functions
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadPlayer();
            LoadSpellList();
            SetFilterButtons();
            DisplayFullSpellList();
            SaveSpellList();
            RefreshPlayerData();
        }

        public List<Spell> SpellList = new List<Spell>();

        public string profileURL;
        public HtmlDocument doc;

        public void SetFilterButtons()
        {
            FilterButtonPanel.Children.Clear();
            Button btn = NewFilterButton("All");
            btn.Click += (sender, e) =>
            {
                DisplayFullSpellList();
            };
            FilterButtonPanel.Children.Add(btn);

            List<string> originalTypeList = new List<string>();
            for (int i = 0; i < SpellList.Count; i++)
                originalTypeList.Add(SpellList[i].type);
            List<string> consolidatedTypeList = originalTypeList.Distinct().ToList();

            foreach (string a in consolidatedTypeList)
            {
                btn = NewFilterButton(a);
                btn.Click += (sender, e) =>
                {
                    FilterButton_Click(sender, e);
                };
                FilterButtonPanel.Children.Add(btn);
            }

        }

        public Button NewFilterButton(string name)
        {
            Button btn = new Button
            {
                Content = name,
                Margin = new Thickness(0),
                MinWidth = 60,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.AntiqueWhite,
                Style = this.FindResource("MyButtonStyle") as Style,
                Background = new ImageBrush
                {
                    ImageSource = GetImage("Images/tab.png")
                }
            };
            btn.MouseEnter += Btn_MouseEnter;
            btn.MouseLeave += Btn_MouseLeave;
            return btn;
        }

        private void Btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = (sender as Button);
            button.Background = new ImageBrush
            {
                ImageSource = GetImage("Images/tab.png")
            };
        }

        private void Btn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = (sender as Button);
            button.Background = new ImageBrush
            {
                ImageSource = GetImage("Images/tabSelected.png")
            };

        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Content.ToString();
            DisplayFilteredSpellList(content);
        }

        public class Spell
        {
            public string name;
            public string description;
            public string type;
            public string movements;

            public Spell() { }

            public Spell(string spellName, string spellDescription, string spellType, string spellMovements)
            {
                name = spellName;
                description = spellDescription;
                type = spellType;
                movements = spellMovements;
            }
        }

        public void DisplayFullSpellList()
        {
            spellBookPanel.Children.Clear();
            RefreshHtmlDoc();
            for (int i = 0; i < SpellList.Count; i++)
                AddNewSpellPanel(i);
        }

        public void DisplayFilteredSpellList(string filter)
        {
            spellBookPanel.Children.Clear();
            RefreshHtmlDoc();
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].type == filter)
                    AddNewSpellPanel(i);
            }
        }
        
        public void AddNewSpellPanel(int i)
        {
            Spell thisSpell = SpellList[i];
            SpellData thisSpellData = GetSpellData(thisSpell.name);
            
            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(6),
                Background = new ImageBrush
                {
                    ImageSource = GetImage("Images/spellPanelBackground.png")
                }
            };

            //Top Row
            StackPanel spTop = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            spTop.Children.Add(Lb_White_Width(thisSpell.name, 200, new Thickness(8, 4, 0, 0), HorizontalAlignment.Left));
            spTop.Children.Add(Lb_White("Movements: " + thisSpell.movements, new Thickness(0, 4, 0, 0)));
            sp.Children.Add(spTop);

            //Middle Row
            sp.Children.Add(Lb_White("Info: " + thisSpell.description, new Thickness(8, 0, 0, 0)));

            //Bottom Row
            StackPanel spBottom = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            spBottom.Children.Add(Lb_White_Width(thisSpell.type, 120, new Thickness(8, 2, 0, 8), HorizontalAlignment.Left));
            spBottom.Children.Add(Lb_White_Width(thisSpellData.level, 160, new Thickness(0, 2, 0, 8), HorizontalAlignment.Right));
            ProgressBar pbProgress = new ProgressBar
            {
                Value = Convert.ToDouble(thisSpellData.percentage),
                Width = 260,
                Height = 16,
                Margin = new Thickness(0, 0, 0, 6)
            };
            spBottom.Children.Add(pbProgress);
            sp.Children.Add(spBottom);

            spellBookPanel.Children.Add(sp);
        }

        private Label Lb_White_Width(string lbContent, int lbWidth, Thickness lbMargin, HorizontalAlignment lbAlignment)
        {

            Label lb = new Label
            {
                Content = lbContent,
                Foreground = Brushes.DarkBlue,
                Width = lbWidth,
                Margin = lbMargin,
                FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Kalam"),
                FontSize = 20,
                HorizontalContentAlignment = lbAlignment
            };
            return lb;
        }

        private Label Lb_White(string lbContent, Thickness lbMargin)
        {
            Label lb = new Label
            {
                Content = lbContent,
                Foreground = Brushes.DarkBlue,
                Margin = lbMargin,
                FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Kalam"),
                FontSize = 20
            };
            return lb;
        }

        private static BitmapImage GetImage(string imageUri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri("pack://application:,,,/" + imageUri, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            return bitmapImage;
        }

        private void RefreshHtmlDoc()
        {
            string url = profileURL;
            var web = new HtmlAgilityPack.HtmlWeb();

            System.Diagnostics.Debug.WriteLine(url);

            try
            {
                System.Diagnostics.Debug.WriteLine("Try Load");
                doc = web.Load(url);
                SaveSpellData();
                System.Diagnostics.Debug.WriteLine("Saved");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Try Failed");
                LoadSpellData();
            }
        }

        struct SpellData
        {
            public string percentage;
            public string level;
        }

        private SpellData GetSpellData(string spellName)
        {
            SpellData spell = new SpellData
            {
                percentage = "0",
                level = "0"
            };

            int knownSpellCount = doc.DocumentNode.SelectNodes("//table[@id='spell-data'][1]//tbody//tr").Count;

            for (int i = 1; i <= knownSpellCount; i++)
            {
                string spellID = doc.DocumentNode.SelectSingleNode("//*[@id=\"spell-data\"]/tbody/tr[" + i + "]/th").InnerText;

                //System.Diagnostics.Debug.WriteLine("checking comparison of " + spellID + " and " + spellName);

                if (String.Equals(spellID, spellName, StringComparison.OrdinalIgnoreCase))
                {
                    spell.percentage = doc.DocumentNode
                        .SelectSingleNode("//*[@id=\"spell-data\"]/tbody/tr[" + i + "]/td/div")
                        .Attributes["data-percent"].Value;
                    spell.level = doc.DocumentNode
                        .SelectSingleNode("//*[@id=\"spell-data\"]/tbody/tr[" + i + "]/td/div")
                        .Attributes["data-content"].Value;

                    return spell;
                }
            }

            return spell;
        }

        public void SaveSpellList()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpellBook");
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
                info.Create();

            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(List<Spell>));

            var path2 = System.IO.Path.Combine(path, "SpellList.xml");
            System.IO.FileStream file = System.IO.File.Create(path2);

            writer.Serialize(file, SpellList);
            file.Close();
        }

        public void LoadSpellList()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpellBook");
            var path2 = System.IO.Path.Combine(path, "SpellList.xml");

            if (File.Exists(path2))
            {
                System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(List<Spell>));
                System.IO.StreamReader file = new System.IO.StreamReader(path2);
                SpellList = (List<Spell>)reader.Deserialize(file);
                file.Close();
            }
            else
            {
                StartUpSpellList();
            }    
        }

        public void StartUpSpellList()
        {
            string[] spellNames = { "Lumos"};
            string[] spellTypes = { "Charm"};
            string[] spellMovements = { "Up"};
            string[] spellDescriptions = { "Casts light around the player"};
            
            for (int i = 0; i < spellNames.Length; i++)
            {
                Spell newSpell = new Spell(spellNames[i], spellDescriptions[i], spellTypes[i], spellMovements[i]);
                SpellList.Add(newSpell);
            }
        }

        public void SaveSpellData()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpellBook");
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
                info.Create();
            var path2 = System.IO.Path.Combine(path, "SpellData.html");



            FileStream sw = new FileStream(path2, FileMode.Create);
            doc.Save(sw);
            sw.Close();
        }

        public void LoadSpellData()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpellBook");
            var path2 = System.IO.Path.Combine(path, "SpellData.html");

            if (File.Exists(path2))
            {
                FileStream sw = new FileStream(path2, FileMode.Open);
                doc.Load(sw);
                sw.Close();
            }
        }

        public void RefreshPlayerData()
        {
            PlayerData player = GetPlayerData();

            nameLbl.Content = player.name;
            yearLbl.Content = player.year;
            houseLbl.Content = player.house;
            pointsLbl.Content = player.housePoints;

        }

        struct PlayerData
        {
            public string name;
            public string year;
            public string house;
            public string housePoints;
        }

        private PlayerData GetPlayerData()
        {
            PlayerData data = new PlayerData
            {
                name = "0",
                year = "0",
                house = "0",
                housePoints = "0"
            };

            data.name = doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[1]/h2/text()").InnerText;
            data.year = doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[2]/div[3]/div").InnerText;
            data.house = doc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[2]/div[2]/div").InnerText;
            data.housePoints = doc.DocumentNode.SelectSingleNode("//*[@id=\"user-data\"]/tbody/tr[4]/td").InnerText;

            return data;
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshPlayerData();
        }



        private void SpellSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            Spell newSpell = new Spell
            {
                name = spellNameEntry.Text,
                description = spellDescriptionEntry.Text,
                type = spellTypeEntry.Text,
                movements = spellMovementsEntry.Text
            };

            SpellList.Add(newSpell);
            SaveSpellList();
            ClearSpellEntry();
            addSpellGrid.Visibility = Visibility.Collapsed;
            SetFilterButtons();
            DisplayFullSpellList();
        }

        private void AddSpellButton_Click(object sender, RoutedEventArgs e)
        {
            addSpellGrid.Visibility = Visibility.Visible;


        }

        private void SpellCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearSpellEntry();
            addSpellGrid.Visibility = Visibility.Collapsed;
        }

        private void ClearSpellEntry()
        {
            spellNameEntry.Text = "";
            spellMovementsEntry.Text = "";
            spellTypeEntry.Text = "";
            spellDescriptionEntry.Text = "";
        }

        private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            addPlayerGrid.Visibility = Visibility.Visible;
        }

        private void PlayerSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            profileURL = playerUrlEntry.Text;
            System.Diagnostics.Debug.WriteLine(profileURL);
            SavePlayer();
            System.Diagnostics.Debug.WriteLine(profileURL);
            addPlayerGrid.Visibility = Visibility.Collapsed;
            RefreshHtmlDoc();
            RefreshPlayerData();
            DisplayFullSpellList();
            System.Diagnostics.Debug.WriteLine(profileURL);
            
        }
        private void PlayerCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            addPlayerGrid.Visibility = Visibility.Collapsed;
        }

        public void LoadPlayer()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpellBook");
            var path2 = System.IO.Path.Combine(path, "Player.xml");

            if (File.Exists(path2))
            {
                System.Diagnostics.Debug.WriteLine("File Exists");
                System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(string));
                System.IO.StreamReader file = new System.IO.StreamReader(path2);
                profileURL = (string)reader.Deserialize(file);
                file.Close();
                SavePlayer();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("File Does Not Exist");
                DefaultPlayer();
            }

            RefreshHtmlDoc();
        }

        public void DefaultPlayer()
        {
            profileURL = "http://profile.knockturnmc.com/player/49f1ee42-854c-45cb-bc6b-e307ed0bc8e7";
            SavePlayer();
        }

        public void SavePlayer()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpellBook");
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
                info.Create();

            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(string));

            var path2 = System.IO.Path.Combine(path, "Player.xml");
            System.IO.FileStream file = System.IO.File.Create(path2);

            writer.Serialize(file, profileURL);
            file.Close();
        }

    }
}
