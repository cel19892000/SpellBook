using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using System.Globalization;
using System.Windows.Input;

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
            DisplayFullSpellList();
            sm.SaveSpellList(SpellList);
            RefreshPlayerData();
        }

        readonly SaveManager sm = new SaveManager();

        public List<Spell> SpellList = new List<Spell>();

        public string profileURL;
        public HtmlDocument doc;
        public string lastPrimaryFilterPressed = "";
        public string lastSecondaryFilterPressed = "";
        public string lastFilterPressed = "";

        public bool AreMovementsHidden
        {
            get { return (bool)GetValue(AreMovementsHiddenProperty); }
            set { SetValue(AreMovementsHiddenProperty, value); }
        }

        public static readonly DependencyProperty AreMovementsHiddenProperty =
            DependencyProperty.Register("AreMovementsHidden", typeof(bool),
            typeof(MainWindow), new UIPropertyMetadata(false));

        public void SetFilterButtons()
        {
            FilterButtonPanel.Children.Clear();
            Button btn = NewFilterButton("All");
            btn.Background = new ImageBrush { ImageSource = GetImage("Images/buttonRed.png") };
            btn.MouseEnter += BtnGreen;
            btn.MouseLeave += WhatIdleColorIsPrimary;
            btn.Click += (sender, e) =>
            {
                lastPrimaryFilterPressed = "All";
                lastFilterPressed = "";
                FilterAction();
            };
            FilterButtonPanel.Children.Add(btn);

            List<string> originalTypeList = new List<string>();
            for (int i = 0; i < SpellList.Count; i++)
            {
                string type = new SpellType(SpellList[i].type).primary;
                originalTypeList.Add(type);
            }
                
            List<string> consolidatedTypeList = originalTypeList.Distinct().ToList();

            foreach (string a in consolidatedTypeList)
            {
                btn = NewFilterButton(a);
                btn.Click += (sender, e) =>
                {
                    FilterButton_Click(sender, e);
                };
                btn.Background = new ImageBrush { ImageSource = GetImage("Images/buttonRed.png") };
                btn.MouseEnter += BtnGreen;
                btn.MouseLeave += WhatIdleColorIsPrimary;
                FilterButtonPanel.Children.Add(btn);
            }

        }

        public void SetSecondaryFilterButtons(string primary)
        {
            SecondaryFilterButtonPanel.Children.Clear();
            TypeSelectedLbl.Visibility = Visibility.Collapsed;

            List<string> originalTypeList = new List<string>();
            for (int i = 0; i < SpellList.Count; i++)
            {
                string primaryType = new SpellType(SpellList[i].type).primary;
                string type = new SpellType(SpellList[i].type).secondary;
                if (type != null && primary.Equals(primaryType))
                    originalTypeList.Add(type);
            }

            List<string> consolidatedTypeList = originalTypeList.Distinct().ToList();

            foreach (string a in consolidatedTypeList)
            {
                Button btn = NewFilterButton(a);
                btn.Click += (sender, e) =>
                {
                    SecondaryFilterButton_Click(sender, e);
                };
                btn.Background = new ImageBrush { ImageSource = GetImage("Images/buttonPurple.png") };
                btn.MouseEnter += BtnGreen;
                btn.MouseLeave += WhatIdleColorIsSecondary;
                SecondaryFilterButtonPanel.Children.Add(btn);
                TypeSelectedLbl.Content = primary;
                TypeSelectedLbl.Visibility = Visibility.Visible;
            }
        }

        public void WhatIdleColorIsPrimary(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as Button).Content.Equals(lastPrimaryFilterPressed))
                BtnGreen(sender, e);
            else
                BtnRed(sender, e);
        }

        public void SetPrimaryButtonColors()
        {
            foreach (Button a in FilterButtonPanel.Children)
            {
                if (a.Content.Equals(lastPrimaryFilterPressed))
                {
                    a.Background = new ImageBrush
                    {
                        ImageSource = GetImage("Images/buttonGreen.png")
                    };
                }
                else
                {
                    a.Background = new ImageBrush
                    {
                        ImageSource = GetImage("Images/buttonRed.png")
                    };
                }
            }
        }

        public void WhatIdleColorIsSecondary(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as Button).Content.Equals(lastSecondaryFilterPressed))
                BtnGreen(sender, e);
            else
                BtnPurple(sender, e);
        }

        public void SetSecondaryButtonColors()
        {
            foreach (Button a in SecondaryFilterButtonPanel.Children)
            {
                if (a.Content.Equals(lastSecondaryFilterPressed))
                {
                    a.Background = new ImageBrush
                    {
                        ImageSource = GetImage("Images/buttonGreen.png")
                    };
                }
                else
                {
                    a.Background = new ImageBrush
                    {
                        ImageSource = GetImage("Images/buttonPurple.png")
                    };
                }
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
                Style = this.FindResource("MyButtonStyle") as Style
            };
            return btn;
        }

        private void BtnGreen(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = (sender as Button);
            button.Background = new ImageBrush
            {
                ImageSource = GetImage("Images/buttonGreen.png")
            };
        }

        private void BtnRed(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = (sender as Button);
            button.Background = new ImageBrush
            {
                ImageSource = GetImage("Images/buttonRed.png")
            };
        }

        private void BtnPurple(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = (sender as Button);
            button.Background = new ImageBrush
            {
                ImageSource = GetImage("Images/buttonPurple.png")
            };
        }

        public void AddNewSpellPanel(int i)
        {
            Spell thisSpell = SpellList[i];
            SpellProgress thisSpellData = new SpellProgress(thisSpell.name, doc);
            
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
            if (!AreMovementsHidden)
                spTop.Children.Add(Lb_White("Movements: " + thisSpell.movements, new Thickness(0, 4, 0, 0)));
            else
                spTop.Children.Add(Lb_White("Movements: Hidden", new Thickness(0, 4, 0, 0)));
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
            double percentage = Convert.ToDouble(thisSpellData.percentage, new CultureInfo("en-US"));
            ProgressBar pbProgress = new ProgressBar
            {
                Value = percentage,
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

        private static BitmapImage GetUrlImage(string imageUri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imageUri);
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
            PlayerData player = new PlayerData(doc);
            nameLbl.Content = player.name;
            yearLbl.Content = player.year;
            houseLbl.Content = player.house;
            pointsLbl.Content = player.housePoints;
            PlayerImage.Source = GetUrlImage(player.playerImageSource);
            TotalSpellsLbl.Content = SpellList.Count;
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e) => RefreshPlayerData();

        private void SpellSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            Spell newSpell = new Spell
            {
                name = spellNameEntry.Text,
                description = spellDescriptionEntry.Text,
                type = spellTypeEntry.Text,
                movements = spellMovementsEntry.Text
            };

            if (!SpellExistsAlready(newSpell.name))
            {
                SpellList.Add(newSpell);
                sm.SaveSpellList(SpellList);
                ClearSpellEntry();
                addSpellGrid.Visibility = Visibility.Collapsed;
                SetFilterButtons();
                FilterSpellsByName(newSpell.name);
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

        private void PlayerSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            profileURL = playerUrlEntry.Text;
            System.Diagnostics.Debug.WriteLine(profileURL);
            sm.SavePlayer(profileURL);
            System.Diagnostics.Debug.WriteLine(profileURL);
            addPlayerGrid.Visibility = Visibility.Collapsed;
            RefreshHtmlDoc();
            RefreshPlayerData();
            DisplayFullSpellList();
            System.Diagnostics.Debug.WriteLine(profileURL);
            
        }
        private void PlayerCancelBtn_Click(object sender, RoutedEventArgs e) => addPlayerGrid.Visibility = Visibility.Collapsed;

        public void LoadPlayer()
        {
            profileURL = sm.LoadPlayer();
            sm.SavePlayer(profileURL);
            RefreshHtmlDoc();
        }

        //Spell Filtering

        private void SpellSearchButton_Click(object sender, RoutedEventArgs e) => FilterSpellsByName(SpellSearchBox.Text);

        private void OnKeyDownSpellSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                FilterSpellsByName(SpellSearchBox.Text);
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Content.ToString();
            lastPrimaryFilterPressed = content;
            lastSecondaryFilterPressed = "";
            lastFilterPressed = "Primary";
            FilterAction();
        }

        private void SecondaryFilterButton_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Content.ToString();
            lastSecondaryFilterPressed = content;
            lastFilterPressed = "Secondary";
            FilterAction();
        }

        private void FilterAction()
        {
            if (lastFilterPressed.Equals("Primary"))
            {
                FilterSpellsByPrimaryType(lastPrimaryFilterPressed);
                SetSecondaryFilterButtons(lastPrimaryFilterPressed);
                SetPrimaryButtonColors();
            }
            else if (lastFilterPressed.Equals("Secondary"))
            {
                FilterSpellsBySecondaryType(lastSecondaryFilterPressed);
                SetSecondaryButtonColors();
            }
            else
            {
                DisplayFullSpellList();
                TypeSelectedLbl.Visibility = Visibility.Collapsed;
                SecondaryFilterButtonPanel.Children.Clear();
                SetPrimaryButtonColors();
            }
        }

        public void DisplayFullSpellList()
        {
            ClearSpellBook();
            for (int i = 0; i < SpellList.Count; i++)
                AddNewSpellPanel(i);
        }

        private void FilterSpellsByName(string spellName)
        {
            ClearSpellBook();
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].name.Contains(spellName, StringComparison.OrdinalIgnoreCase))
                    AddNewSpellPanel(i);
            }
        }

        public void FilterSpellsByPrimaryType(string filter)
        {
            ClearSpellBook();
            for (int i = 0; i < SpellList.Count; i++)
            {
                string type = new SpellType(SpellList[i].type).primary;
                if (type == filter)
                    AddNewSpellPanel(i);
            }
        }

        public void FilterSpellsBySecondaryType(string secondary)
        {
            ClearSpellBook();

            for (int i = 0; i < SpellList.Count; i++)
            {
                string type = new SpellType(SpellList[i].type).primary;
                string subType = new SpellType(SpellList[i].type).secondary;
                if (subType != null)
                {
                    if (type.Equals(lastPrimaryFilterPressed) && subType.Equals(secondary))
                    {
                        AddNewSpellPanel(i);
                    }
                }
            }
        }

        public void ClearSpellBook()
        {
            spellBookPanel.Children.Clear();
            RefreshHtmlDoc();
        }

        public bool SpellExistsAlready(string spellName)
        {
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].name.Equals(spellName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }


        //Edit Spell Menu
        private void EditSpellButton_Click(object sender, RoutedEventArgs e) => editSpellGrid.Visibility = Visibility.Visible;

        private void EditSpellSearchButton_Click(object sender, RoutedEventArgs e) => DisplayEditableSpell(editSpellSearchBox.Text);

        public void DisplayEditableSpell(string searchedSpell)
        {
            int spellID = SelectFirstMatchingSpellID(searchedSpell);

            if (spellID != 9999)
            {
                EditSpellBoxes(SpellList[spellID].name, SpellList[spellID].type, SpellList[spellID].movements, SpellList[spellID].description, spellID);
            }
            else
            {
                EditSpellBoxes("Spell Not Found", "", "", "", 9999);
            }
        }

        public int SelectFirstMatchingSpellID(string searchedSpell)
        {
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].name.Contains(searchedSpell, StringComparison.OrdinalIgnoreCase))
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
                SetFilterButtons();
                FilterSpellsByName(editedSpell.name);
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
            Spell gatheredSpell = new Spell(editSpellNameBox.Text, editSpellDescriptionBox.Text, editSpellTypeBox.Text, editSpellMovementsBox.Text);
            return gatheredSpell;
        }

        private void OverwriteSpell(Spell editedSpell, int spellID)
        {
            SpellList[spellID] = editedSpell;
        }

        private void HideMovementsCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            FilterAction();
        }
    }
}
