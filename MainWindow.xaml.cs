using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using System.IO;
using System.Globalization;
using System.Windows.Input;

namespace SpellBook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// To Do List
    /// Spell Entry Graphics
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
            sm.SavePlayer(profileURL);
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
            profileURL = sm.LoadPlayer();
            sm.SavePlayer(profileURL);
            RefreshHtmlDoc();
        }

        private void SpellSearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterSpellsByName(SpellSearchBox.Text);
        }
        private void OnKeyDownSpellSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                FilterSpellsByName(SpellSearchBox.Text);
            }
        }

        private void FilterSpellsByName(string spellName)
        {
            spellBookPanel.Children.Clear();
            RefreshHtmlDoc();
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].name.Contains(spellName, StringComparison.OrdinalIgnoreCase))
                    AddNewSpellPanel(i);
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

        public bool SpellExistsAlready(string spellName)
        {
            for (int i = 0; i < SpellList.Count; i++)
            {
                if (SpellList[i].name.Equals(spellName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

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

        private void EditSpellSearchButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayEditableSpell(editSpellSearchBox.Text);
        }

        private void EditSpellButton_Click(object sender, RoutedEventArgs e)
        {
            editSpellGrid.Visibility = Visibility.Visible;
        }

        private void EditSpellCancelButton_Click(object sender, RoutedEventArgs e)
        {
            EditSpellBoxes("No Spell Selected", "", "", "", 9999);
            editSpellGrid.Visibility = Visibility.Collapsed;
        }

        private void EditSpellBoxes(string name, string type, string movements, string description, int id)
        {
            editSpellNameBox.Text = name;
            editSpellTypeBox.Text = type;
            editSpellMovementsBox.Text = movements;
            editSpellDescriptionBox.Text = description;
            editSpellIDLbl.Content = id;
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
                FilterSpellsByName(editedSpell.name);
            }
            else
            {
                MessageBox.Show(Application.Current.MainWindow, "No Spell Selected", "Overwrite Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
    }
}
