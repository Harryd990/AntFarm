using AntFarm.main;
using AntFarm.handelers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AntFarm
{
    
    public partial class StartUpWIndow : Window
    {
        Game newgame;
        
        public StartUpWIndow()
        {
            InitializeComponent();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            (newgame, bool isValid) = Gamestart.HandleNewGame(newgame, (int)GridWidthSlider.Value, (int)GridHeightSlider.Value, (int)AntCountSlider.Value, (int)FoodCountSlider.Value);

            if (isValid)
            {
                 MainWindow mainWindow = new MainWindow(newgame);
                 mainWindow.Show();
                 this.Close();
            }
            else
            {
                MessageBox.Show("Too many ants and food for the grid size. Please adjust the values.");
            }
        }

        private void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string saveDirectory = Path.Combine(documentsFolder, "AntFarmSaves");

            if (!Directory.Exists(saveDirectory))
            {
                MessageBox.Show("No saves directory found. Create a valid save first.", "No Saves", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string[] saveFiles = Directory.GetFiles(saveDirectory, "*.json");

            if (saveFiles.Length == 0)
            {
                MessageBox.Show("No save files exist in your save folder yet.", "No Saves", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // popup window for file selection
            Window loadWindow = new Window
            {
                Title = "Select a Save to Load",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            panel.Children.Add(new TextBlock { Text = "Available Saves:", Margin = new Thickness(0, 0, 0, 10) });

            //  ListBox with scrollviewer active
            ListBox fileListBox = new ListBox
            {
                Height = 220,
                Margin = new Thickness(0, 0, 0, 10)
            };
            
          
            ScrollViewer.SetVerticalScrollBarVisibility(fileListBox, ScrollBarVisibility.Auto);

            foreach (string file in saveFiles)
            {
                fileListBox.Items.Add(Path.GetFileName(file));
            }
            // By default select the most recent based on autosave logic or just first
            fileListBox.SelectedIndex = 0;

            panel.Children.Add(fileListBox);

            Button loadBtn = new Button { Content = "Load Game", Width = 100, HorizontalAlignment = HorizontalAlignment.Right };
            loadBtn.Click += (s, args) =>
            {
                string selectedFile = fileListBox.SelectedItem as string;
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    string fullPath = Path.Combine(saveDirectory, selectedFile);
                    
                    // Validate and ensure game isnt corrupted using Gamestart handler
                    (Game? loadedGame, bool isValid) = Gamestart.HandleLoadGame(fullPath);

                    if (isValid && loadedGame != null)
                    {
                        MainWindow mainWindow = new MainWindow(loadedGame);
                        mainWindow.Show();

                        
                        loadWindow.Close();
                        this.Close(); 
                    }
                    else
                    {
                        MessageBox.Show("The save file is corrupted, empty, or failed to process correctly.", "Corrupted Save", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };
            panel.Children.Add(loadBtn);

            loadWindow.Content = panel;
            loadWindow.ShowDialog();
        }

        private void FoodCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }
        private void GridHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }
        private void GridWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }
        private void AntCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }
    }
}
