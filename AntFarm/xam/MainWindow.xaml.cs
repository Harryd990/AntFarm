using AntFarm.handelers;
using AntFarm.main;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AntFarm
{
    public partial class MainWindow : Window
    {
        private Game _game;
        private DispatcherTimer _simTimer;

        public MainWindow(Game game)
        {
            InitializeComponent();
            _game = game;
            _game.Initialise_Game();
            _game.OnSimulationLog += OnGameLogMessage; 

            // 1. Initialize the UI Timer
            _simTimer = new DispatcherTimer();
            _simTimer.Tick += SimTimer_Tick;

            // 2. Initial rendering
            MainCanvas.Loaded += (s, e) => GridRenderer.Render(_game, MainCanvas);
            MainCanvas.SizeChanged += (s, e) => GridRenderer.Render(_game, MainCanvas);

            // 3. Hook up the Speed Slider
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;
        }

        private void SimTimer_Tick(object sender, EventArgs e)
        {
            // Call the newly created single tick method
            _game.UpdateTick();

            // Re-render the visual state on the canvas
            GridRenderer.Render(_game, MainCanvas);

            // Update the UI text
            UpdatesText.Text = $"Simulation Running...\nTick: {_game.tick}\nTasks in Queue: {_game.queue1.tasks.Count}";
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_simTimer == null) return;

            if (SpeedSlider.Value == 0)
            {
                _simTimer.Stop();
                UpdatesText.Text = $"Simulation Paused (Tick: {_game.tick})";
            }
            else
            {
                // Inversely proportional but doubled speed: slider value 1 = 500ms, value 5 = 100ms
                double intervalMs = 100 / SpeedSlider.Value;
                _simTimer.Interval = TimeSpan.FromMilliseconds(intervalMs);
                
                if (!_simTimer.IsEnabled)
                {
                    _simTimer.Start();
                }
            }
        }

        private void createNdestroybut_Click(object sender, RoutedEventArgs e)
        {
            ShowActionsPanel(PanelCreateDestroy);
        }

        private void SettingsBut_click(object sender, RoutedEventArgs e)
        {
            ShowActionsPanel(PanelSettings);
        }

        private void Dev_tools_But_Click(object sender, RoutedEventArgs e)
        {
            ShowActionsPanel(PanelDevTools);
        }

        private void ShowActionsPanel(StackPanel panelToShow)
        {
            PanelCreateDestroy.Visibility = Visibility.Collapsed;
            PanelSettings.Visibility = Visibility.Collapsed;
            PanelDevTools.Visibility = Visibility.Collapsed;

            panelToShow.Visibility = Visibility.Visible;
        }

        // Create and Destroy
        private void BuildFarm_Click(object sender, RoutedEventArgs e) { }
        private void BuildFoodStore_Click(object sender, RoutedEventArgs e) { }
        private void Dig_Click(object sender, RoutedEventArgs e) { }
        private void RemoveBuilding_Click(object sender, RoutedEventArgs e) { }

        // Settings
        private void IdealPop_Click(object sender, RoutedEventArgs e) 
        {
            if (_game == null) return;

            // Create a small custom popup window in code
            Window inputWindow = new Window
            {
                Title = "Ideal Population",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            panel.Children.Add(new TextBlock { Text = "Enter the new ideal population number:", Margin = new Thickness(0,0,0,10) });
            
            // Pre-fill with the current value
            TextBox textBox = new TextBox { Text = _game.IdealPopulation.ToString(), Padding = new Thickness(2) };
            panel.Children.Add(textBox);

            Button submitBtn = new Button { Content = "Save", Width = 80, Margin = new Thickness(0, 15, 0, 0), HorizontalAlignment = HorizontalAlignment.Right };
            submitBtn.Click += (s, args) => 
            {
                if (int.TryParse(textBox.Text, out int result) && result >= 0)
                {
                    _game.IdealPopulation = result;
                    
                    inputWindow.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid positive number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            panel.Children.Add(submitBtn);

            inputWindow.Content = panel;
            inputWindow.ShowDialog();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            
            StartUpWIndow startUpWindow = new StartUpWIndow();
            startUpWindow.Show();
            this.Close();
        }
        private void SaveGame_Click(object sender, RoutedEventArgs e) { }
        
        private void GetStats_Click(object sender, RoutedEventArgs e) 
        {
            if (_game == null) return;

            // Get the list of strings from your Game.statistics() method
            var statsList = _game.statistics();

            // Join them together with line breaks
            string statsMsg = string.Join("\n", statsList);

            // Pop up the message box
            MessageBox.Show(statsMsg, "Current Game Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Dev Tools
        private void SpawnEntity_Click(object sender, RoutedEventArgs e) { }
        private void RemoveArea_Click(object sender, RoutedEventArgs e) { }
        private void GetCellDetails_Click(object sender, RoutedEventArgs e) { }
        private void EditEntity_Click(object sender, RoutedEventArgs e) { }

        private void OnGameLogMessage(string message)
        {
            
            UpdatesText.Text += $"\n- {message}";
        }
    }
}