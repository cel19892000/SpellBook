﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using System.Windows.Input;
using System.IO;

namespace SpellBook
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadPlayer();
            SpellList = sm.LoadSpellList();
            SetFilterButtons();
            RefreshPlayerData();
            sm.SaveSpellList(SpellList);
            FilterAction(lastFilterPressed);
            SpellView.ItemsSource = DisplayedSpellList;
            PrimaryFilterControl.ItemsSource = PrimaryFilterList;
            SecondaryFilterControl.ItemsSource = SecondaryFilterList;
        }

        readonly SaveManager sm = new SaveManager();

        public List<Spell> SpellList = new List<Spell>();
        public List<Spell> DisplayedSpellList = new List<Spell>();
        public List<Filter> PrimaryFilterList = new List<Filter>();
        public List<Filter> SecondaryFilterList = new List<Filter>();

        public string profileURL;
        public static HtmlDocument doc;
        public string lastPrimaryFilterPressed = "";
        public string lastSecondaryFilterPressed = "";
        public string lastFilterPressed = "All";
        public string searchedSpell = "";

        public bool AreMovementsHidden
        {
            get { return (bool)GetValue(AreMovementsHiddenProperty); }
            set { SetValue(AreMovementsHiddenProperty, value); }
        }

        public static readonly DependencyProperty AreMovementsHiddenProperty =
            DependencyProperty.Register("AreMovementsHidden", typeof(bool),
            typeof(MainWindow), new UIPropertyMetadata(false));

        private void PrimaryFilterPressed(object sender, RoutedEventArgs e)
        {
            lastPrimaryFilterPressed = (sender as Button).Content.ToString();
            lastSecondaryFilterPressed = "";
            if (lastPrimaryFilterPressed.Equals("All") || lastPrimaryFilterPressed.Equals("All Spells"))
                FilterAction("All");
            else if (lastPrimaryFilterPressed.Equals(searchedSpell))
                FilterAction("Search");
            else
                FilterAction("Primary");
            
            RefreshSecondaryFilterButtons();
        }

        public void SetFilterButtons()
        {
            PrimaryFilterList.Clear();
            Filter allFilter = new Filter
            {
                Primary = "All",
                Secondary = ""
            };
            PrimaryFilterList.Add(allFilter);
            List<Filter> originalTypeList = new List<Filter>();
            for (int i = 0; i < SpellList.Count; i++)
            {
                Filter type = new Filter { Primary = new SpellType(SpellList[i].Type).primary, Secondary = ""};
                originalTypeList.Add(type);
            }
            List<Filter> consolidatedTypeList = originalTypeList.Distinct().ToList();
            PrimaryFilterList.AddRange(consolidatedTypeList);
        }

        private void RefreshPrimaryFilterButtons()
        {
            SetFilterButtons();
            PrimaryFilterControl.Items.Refresh();
        }

        private void RefreshSecondaryFilterButtons()
        {
            SetSecondaryFilterButtons(lastPrimaryFilterPressed);
            SecondaryFilterControl.Items.Refresh();
        }

        private void SecondaryFilterPressed(object sender, RoutedEventArgs e)
        {
            lastSecondaryFilterPressed = (sender as Button).Content.ToString();
            FilterAction("Secondary");
        }

        public void SetSecondaryFilterButtons(string primary)
        {
            SecondaryFilterList.Clear();
            List<Filter> originalTypeList = new List<Filter>();
            for (int i = 0; i < SpellList.Count; i++)
            {
                Filter type = new Filter { Primary = new SpellType(SpellList[i].Type).primary, Secondary = new SpellType(SpellList[i].Type).secondary };
                if ((type.Primary != null & type.Secondary != "") && primary.Equals(type.Primary))
                        originalTypeList.Add(type);
            }
            List<Filter> consolidatedTypeList = originalTypeList.Distinct().ToList();
            SecondaryFilterList.AddRange(consolidatedTypeList);
        }

        private static BitmapImage GetUrlImage(string imageUri)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imageUri, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public void RefreshHtmlDoc()
        {
            try
            {
                doc = sm.ImportSpellData();
                sm.SaveSpellData(doc);
            }
            catch
            {
                doc = sm.LoadSpellData();
            }
        }

        public void RefreshPlayerData()
        {
            PlayerData player = new PlayerData();
            nameLbl.Content = player.Name;
            yearLbl.Content = player.Year;
            houseLbl.Content = player.House;
            pointsLbl.Content = player.HousePoints;
            PlayerImage.Source = GetUrlImage(player.PlayerImageSource);
            TotalSpellsLbl.Content = SpellList.Count;
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e) => RefreshPlayerData();

        private void SpellSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            Spell newSpell = new Spell()
            {
                Name = spellNameEntry.Text,
                Description = spellDescriptionEntry.Text,
                Type = spellTypeEntry.Text,
                Movements = spellMovementsEntry.Text
            };

            if (!SpellExistsAlready(newSpell.Name))
            {
                SpellList.Add(newSpell);
                sm.SaveSpellList(SpellList);
                ClearSpellEntry();
                addSpellGrid.Visibility = Visibility.Collapsed;
                RefreshPrimaryFilterButtons();
                FilterAction(lastFilterPressed);
            }
            else
            {
                MessageBox.Show(Application.Current.MainWindow, "Spell Already Exists", "Add Spell Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSpellButton_Click(object sender, RoutedEventArgs e) => addSpellGrid.Visibility = Visibility.Visible;

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

        private void AddPlayerButton_Click(object sender, RoutedEventArgs e) => addPlayerGrid.Visibility = Visibility.Visible;
        private void UsernameSearchButtonClick(object sender, RoutedEventArgs e)
        {
            mcUsernameLbl.Content = playerUrlEntry.Text;
            string url = new PlayerFinder(playerUrlEntry.Text).url;
            if (url == "Unknown")
            {
                mcUUIDLbl.Content = "Invalid Minecraft Username";
                knockturnNameLbl.Content = "";
                confirmPlayerButton.Visibility = Visibility.Hidden;
            }
            else
            {
                mcUUIDLbl.Content = new PlayerFinder(playerUrlEntry.Text).uuid;
                HtmlDocument uuidDoc = sm.ImportSpellDataByUrl(url);
                HtmlNodeCollection childNodes = uuidDoc.DocumentNode.SelectSingleNode("//*[@class=\"ui container\"]").ChildNodes;
                foreach (var node in childNodes)
                {
                    if (node.NodeType == HtmlNodeType.Element)
                    {
                        System.Diagnostics.Debug.WriteLine(node.Name);
                        if (node.Name == "h2")
                        {
                            knockturnNameLbl.Content = "Unknown Player";
                            confirmPlayerButton.Visibility = Visibility.Hidden;
                            return;
                        }
                        else
                        {
                            knockturnNameLbl.Content = uuidDoc.DocumentNode.SelectSingleNode("//*[@class=\"ui segments\"]/div[1]/h2/text()").InnerText;
                            confirmPlayerButton.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void PlayerSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            profileURL = new PlayerFinder(playerUrlEntry.Text).url;
            //Check
            System.Diagnostics.Debug.WriteLine(profileURL);
            sm.SavePlayer(profileURL);
            addPlayerGrid.Visibility = Visibility.Collapsed;
            RefreshHtmlDoc();
            RefreshPlayerData();
            FilterAction(lastFilterPressed);
        }

        private void PlayerCancelBtn_Click(object sender, RoutedEventArgs e) => addPlayerGrid.Visibility = Visibility.Collapsed;

        public void LoadPlayer()
        {
            profileURL = sm.LoadPlayer();
            sm.SavePlayer(profileURL);
            RefreshHtmlDoc();
        }

        //Spell Filtering

        private void SpellSearchButton_Click(object sender, RoutedEventArgs e) => SearchBoxEntry(SpellSearchBox.Text);

        private void OnKeyDownSpellSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SearchBoxEntry(SpellSearchBox.Text);
            }
        }

        private void SearchBoxEntry(string spellName)
        {
            searchedSpell = spellName;
            FilterAction("Search");
        }

        private void SecondaryFilterButton_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Content.ToString();
            lastSecondaryFilterPressed = content;
            FilterAction("Secondary");
        }

        private void FilterAction(string filterType)
        {
            ClearSpellBook();
            DisplayedSpellList.Clear();
            lastFilterPressed = filterType;
            if (filterType.Equals("Primary"))
            {
                FilterSpellsByPrimaryType(lastPrimaryFilterPressed);
                SetSecondaryFilterButtons(lastPrimaryFilterPressed);
                TypeSelectedBtn.Content = lastPrimaryFilterPressed;
            }
            else if (filterType.Equals("Secondary"))
            {
                FilterSpellsBySecondaryType(lastSecondaryFilterPressed);
            }
            else if (filterType.Equals("Search"))
            {
                FilterSpellsByName();
                lastPrimaryFilterPressed = "All";
                lastSecondaryFilterPressed = "";
                RefreshSecondaryFilterButtons();
                TypeSelectedBtn.Content = searchedSpell;
            }
            else
            {
                FilterSpellsByAll();
                TypeSelectedBtn.Content = "All Spells";
            }
            SpellView.Items.Refresh();
        }

        private void FilterSpellsByAll()
        {
            for (int i = 0; i < SpellList.Count; i++)
                DisplayedSpellList.Add(ConvertedSpell(i));
        }

        private void FilterSpellsByName()
        {
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].Name.Contains(searchedSpell, StringComparison.OrdinalIgnoreCase))
                    DisplayedSpellList.Add(ConvertedSpell(i));
            }
        }

        public Spell ConvertedSpell(int i)
        {
            Spell newSpell;
            if (AreMovementsHidden)
                newSpell = new Spell() { Name = SpellList[i].Name, Description = SpellList[i].Description, Type = SpellList[i].Type, Movements = "Hidden" };
            else
                newSpell = new Spell() { Name = SpellList[i].Name, Description = SpellList[i].Description, Type = SpellList[i].Type, Movements = SpellList[i].Movements };
            return newSpell;
        }

        public void FilterSpellsByPrimaryType(string filter)
        {
            for (int i = 0; i < SpellList.Count; i++)
            {
                string type = new SpellType(SpellList[i].Type).primary;
                if (type == filter)
                    DisplayedSpellList.Add(ConvertedSpell(i));
            }
        }

        public void FilterSpellsBySecondaryType(string secondary)
        {
            for (int i = 0; i < SpellList.Count; i++)
            {
                string type = new SpellType(SpellList[i].Type).primary;
                string subType = new SpellType(SpellList[i].Type).secondary;
                if (subType != null)
                {
                    if (type.Equals(lastPrimaryFilterPressed) && subType.Equals(secondary))
                    {
                        DisplayedSpellList.Add(ConvertedSpell(i));
                    }
                }
            }
        }

        public void ClearSpellBook()
        {
            RefreshHtmlDoc();
            SpellList.Sort();
        }

        public bool SpellExistsAlready(string spellName)
        {
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].Name.Equals(spellName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }


        //Edit Spell Menu
        private void EditSpellButtonClick(object sender, RoutedEventArgs e)
        {
            editSpellGrid.Visibility = Visibility.Visible;
            String spellName = (sender as Button).Tag.ToString();
            DisplayEditableSpell(spellName);
        }

        public void DisplayEditableSpell(string searchedSpell)
        {
            int spellID = FindExactMatchingSpellID(searchedSpell);
            if (spellID != 9999)
                EditSpellBoxes(SpellList[spellID].Name, SpellList[spellID].Type, SpellList[spellID].Movements, SpellList[spellID].Description, spellID);
            else
                EditSpellBoxes("Spell Not Found", "", "", "", 9999);
        }

        public int FindExactMatchingSpellID(string searchedSpell)
        {
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].Name.Equals(searchedSpell))
                    return i;
            }
            return 9999;
        }

        private void EditSpellCancelButton_Click(object sender, RoutedEventArgs e)
        {
            EditSpellBoxes("No Spell Selected", "", "", "", 9999);
            editSpellGrid.Visibility = Visibility.Collapsed;
        }

        private void EditSpellSaveButton_Click(object sender, RoutedEventArgs e)
        {
            int spellID = Convert.ToInt32(editSpellIDLbl.Content);
            if (spellID != 9999)
            {
                Spell editedSpell = GatherSelectedSpell();
                OverwriteSpell(editedSpell, spellID);
                EditSpellBoxes("No Spell Selected", "", "", "", 9999);
                editSpellGrid.Visibility = Visibility.Collapsed;
                sm.SaveSpellList(SpellList);
                RefreshPrimaryFilterButtons();
                FilterAction(lastFilterPressed);
            }
            else
            {
                MessageBox.Show(Application.Current.MainWindow, "No Spell Selected", "Overwrite Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditSpellBoxes(string name, string type, string movements, string description, int id)
        {
            editSpellNameBox.Text = name;
            editSpellTypeBox.Text = type;
            editSpellMovementsBox.Text = movements;
            editSpellDescriptionBox.Text = description;
            editSpellIDLbl.Content = id;
        }

        private Spell GatherSelectedSpell()
        {
            Spell gatheredSpell = new Spell()
            {
                Name = editSpellNameBox.Text,
                Description = editSpellDescriptionBox.Text,
                Type = editSpellTypeBox.Text,
                Movements = editSpellMovementsBox.Text
            };
            return gatheredSpell;
        }

        private void OverwriteSpell(Spell editedSpell, int spellID) => SpellList[spellID] = editedSpell;

        private void HideMovementsCheckBoxChanged(object sender, RoutedEventArgs e) => FilterAction(lastFilterPressed);

        

    }
}
