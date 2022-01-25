using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace SpellBook
{
    public partial class MainWindow : Window
    {
        readonly SaveManager sm = new SaveManager();

        public MainWindow()
        {
            InitializeComponent();
            data = sm.Load();
            RefreshHtmlDoc();
            RefreshPlayerData();
            SetFilterButtons();
            FilterAction(lastFilterPressed);
            SetSources();
            sm.Save(data);
        }

        public void SetSources()
        {
            SpellView.ItemsSource = DisplayedSpellList;
            PrimaryFilterControl.ItemsSource = PrimaryFilterList;
            SecondaryFilterControl.ItemsSource = SecondaryFilterList;
        }

        public List<Spell> DisplayedSpellList = new List<Spell>();
        public List<Filter> PrimaryFilterList = new List<Filter>();
        public List<Filter> SecondaryFilterList = new List<Filter>();

        public static SaveData data = new SaveData();

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
            PrimaryFilterList.Add(new Filter { Primary = "All", Secondary = "" });
            List<Filter> originalTypeList = new List<Filter>();
            data.SpellList.ForEach(spell => originalTypeList.Add(new Filter { Primary = spell.Primary, Secondary = "" }));
            PrimaryFilterList.AddRange(originalTypeList.Distinct().ToList());
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
            foreach(Spell spell in data.SpellList)
            {
                Filter filter = new Filter { Primary = spell.Primary, Secondary = spell.Secondary };
                if (filter.Secondary != "" && primary.Equals(filter.Primary))
                    originalTypeList.Add(filter);
            }
            SecondaryFilterList.AddRange(originalTypeList.Distinct().ToList());
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
            data.KnockturnData = sm.ImportSpellDataByUrl(data.PlayerUrl);
            sm.Save(data);
        }

        public void RefreshPlayerData()
        {
            PlayerData player = new PlayerData();
            nameLbl.Content = player.Name;
            yearLbl.Content = player.Year;
            houseLbl.Content = player.House;
            pointsLbl.Content = player.HousePoints;
            PlayerImage.Source = GetUrlImage(player.PlayerImageSource);
            TotalSpellsLbl.Content = data.SpellList.Count;
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e) => RefreshPlayerData();

        private void SpellSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            Spell newSpell = new Spell()
            {
                Name = spellNameEntry.Text,
                Description = spellDescriptionEntry.Text,
                Primary = spellPrimaryEntry.Text,
                Secondary = spellSecondaryEntry.Text,
                Movements = spellMovementsEntry.Text
            };

            if (!SpellExistsAlready(newSpell.Name))
            {
                data.SpellList.Add(newSpell);
                sm.Save(data);
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
            spellPrimaryEntry.Text = "";
            spellSecondaryEntry.Text = "";
            spellDescriptionEntry.Text = "";
        }

        private void AddPlayerButton_Click(object sender, RoutedEventArgs e) => addPlayerGrid.Visibility = Visibility.Visible;
        private void UsernameSearchButtonClick(object sender, RoutedEventArgs e)
        {
            confirmPlayerButton.Visibility = Visibility.Hidden;
            mcUsernameLbl.Content = playerUrlEntry.Text;
            PlayerFinder player = new PlayerFinder(playerUrlEntry.Text);

            if (player.uuid == "Unknown")
            {
                mcUUIDLbl.Content = "Invalid Minecraft Username";
                knockturnNameLbl.Content = "";
            }
            else
            {
                mcUUIDLbl.Content = player.uuid;
                if (player.knockturnName == "Unknown")
                {
                    knockturnNameLbl.Content = "Unknown Player";
                }
                else
                {
                    knockturnNameLbl.Content = player.knockturnName;
                    confirmPlayerButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void PlayerSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            data.PlayerUrl = new PlayerFinder(playerUrlEntry.Text).GetPlayerUrl();
            sm.Save(data);
            addPlayerGrid.Visibility = Visibility.Collapsed;
            RefreshHtmlDoc();
            RefreshPlayerData();
            FilterAction(lastFilterPressed);
        }

        private void PlayerCancelBtn_Click(object sender, RoutedEventArgs e) => addPlayerGrid.Visibility = Visibility.Collapsed;

        //Spell Filtering
        private void SpellSearchButton_Click(object sender, RoutedEventArgs e) => SearchBoxEntry(SpellSearchBox.Text);

        private void OnKeyDownSpellSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                SearchBoxEntry(SpellSearchBox.Text);
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
            foreach (Spell spell in data.SpellList)
                DisplayedSpellList.Add(ConvertedSpell(spell));
        }

        private void FilterSpellsByName()
        {
            foreach(Spell spell in data.SpellList) 
                if (spell.Name.Contains(searchedSpell, StringComparison.OrdinalIgnoreCase))
                    DisplayedSpellList.Add(ConvertedSpell(spell));
        }

        public Spell ConvertedSpell(Spell spell)
        {
            Spell newSpell;
            if (AreMovementsHidden)
                newSpell = new Spell() 
                { 
                    Name = spell.Name, 
                    Description = spell.Description,
                    Primary = spell.Primary,
                    Secondary = spell.Secondary,
                    Movements = "Hidden" 
                };
            else
                newSpell = new Spell()
                {
                    Name = spell.Name, 
                    Description = spell.Description,
                    Primary = spell.Primary,
                    Secondary = spell.Secondary,
                    Movements = spell.Movements 
                };
            return newSpell;
        }

        public void FilterSpellsByPrimaryType(string filter)
        {
            foreach (Spell spell in data.SpellList)
                if(filter.Equals(spell.Primary))
                    DisplayedSpellList.Add(ConvertedSpell(spell));
        }

        public void FilterSpellsBySecondaryType(string secondary)
        {
            foreach (Spell spell in data.SpellList)
            {
                if (spell.Secondary != null && spell.Primary.Equals(lastPrimaryFilterPressed) && spell.Secondary.Equals(secondary))
                    DisplayedSpellList.Add(ConvertedSpell(spell));
            }
        }

        public void ClearSpellBook()
        {
            RefreshHtmlDoc();
            data.SpellList.Sort();
        }

        public bool SpellExistsAlready(string spellName) => data.SpellList.Where(i => i.Name.Equals(spellName, StringComparison.OrdinalIgnoreCase)).Any();

        //Edit Spell Menu
        private void EditSpellButtonClick(object sender, RoutedEventArgs e)
        {
            editSpellGrid.Visibility = Visibility.Visible;
            foreach (Spell spell in data.SpellList)
                if (spell.Name.Equals((sender as Button).Tag.ToString()))
                    EditSpellBoxes(spell.Name, spell.Primary, spell.Secondary, spell.Movements, spell.Description, data.SpellList.IndexOf(spell));
        }

        private void EditSpellCancelButton_Click(object sender, RoutedEventArgs e) => editSpellGrid.Visibility = Visibility.Collapsed;

        private void EditSpellSaveButton_Click(object sender, RoutedEventArgs e)
        {
            data.SpellList[Convert.ToInt32(editSpellIDLbl.Content)] = GatherSelectedSpell();
            editSpellGrid.Visibility = Visibility.Collapsed;
            sm.Save(data);
            RefreshPrimaryFilterButtons();
            FilterAction(lastFilterPressed);
        }

        private void EditSpellBoxes(string name, string primary, string secondary, string movements, string description, int id)
        {
            editSpellNameBox.Text = name;
            editSpellPrimaryBox.Text = primary;
            editSpellSecondaryBox.Text = secondary;
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
                Primary = editSpellPrimaryBox.Text,
                Secondary = editSpellSecondaryBox.Text,
                Movements = editSpellMovementsBox.Text
            };
            return gatheredSpell;
        }

        private void HideMovementsCheckBoxChanged(object sender, RoutedEventArgs e) => FilterAction(lastFilterPressed);

    }
}
