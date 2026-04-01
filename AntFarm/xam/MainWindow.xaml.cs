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
            // Call your existing game tick method
            _game.Run();

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
                // Inversely proportional: slider value 1 = 1000ms, value 5 = 200ms
                double intervalMs = 1000.0 / SpeedSlider.Value;
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
        private void IdealPop_Click(object sender, RoutedEventArgs e) { }
        private void ExitGame_Click(object sender, RoutedEventArgs e) { }
        private void SaveGame_Click(object sender, RoutedEventArgs e) { }
        private void GetStats_Click(object sender, RoutedEventArgs e) { }

        // Dev Tools
        private void SpawnEntity_Click(object sender, RoutedEventArgs e) { }
        private void RemoveArea_Click(object sender, RoutedEventArgs e) { }
        private void GetCellDetails_Click(object sender, RoutedEventArgs e) { }
        private void EditEntity_Click(object sender, RoutedEventArgs e) { }
    }
}