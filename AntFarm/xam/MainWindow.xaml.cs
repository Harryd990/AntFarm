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
        
         private void Dig_Click(object sender, RoutedEventArgs e)
        {
           
            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();
                
                // If the user right-clicked, the result is (-1, -1), so we break out of the loop
                if (gridX == -1 || gridY == -1) 
                {
                    UpdatesText.Text += "\n- Exited Dig Tool";
                    break;
                }

                string typeName = _game.GetCellType(gridX, gridY);
                
                // Allow both Dirt and stone
                if (typeName == "Dirt" || typeName.Equals("stone", StringComparison.OrdinalIgnoreCase)) 
                {
                    AntFarm.algorithm.Task digtask = new AntFarm.algorithm.Task(_game.queue1.lasttaskid++, "dig", (gridX, gridY));
                    _game.queue1.addtask(digtask);
                    UpdatesText.Text += $"\n- Queued dig task at ({gridX}, {gridY})";
                    
                    // Ensure visual update right away
                    GridRenderer.Render(_game, MainCanvas);
                }
                else
                {
                    MessageBox.Show("You can only dig through dirt or stone cells.", "Invalid Target", MessageBoxButton.OK, MessageBoxImage.Warning);
                }   
            }
        }
        
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
            // Create the specific log line we want to append
            string logLine = $"\n- {message}";
            UpdatesText.Text += logLine;

            // Scroll to the bottom to make sure the user sees the death notification
            if (UpdatesText.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
            }

            // Start a fire-and-forget background task to remove this specific line after 5 seconds
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(5000);

                // We must use Dispatcher.Invoke because we are touching UI elements from a background thread!
                Dispatcher.Invoke(() =>
                {
                    // Find and remove the first occurrence of this exact log line
                    int index = UpdatesText.Text.IndexOf(logLine);
                    if (index >= 0)
                    {
                        UpdatesText.Text = UpdatesText.Text.Remove(index, logLine.Length);
                    }
                });
            });
        }
        public (int, int) canvasCellSelect()
        {
            var frame = new DispatcherFrame();
            (int x, int y) result = (-1, -1);

            System.Windows.Input.MouseButtonEventHandler clickHandler = null;
            System.Windows.Input.MouseButtonEventHandler rightClickHandler = null;
            
            // Add a red border using a WPF Adorner so the GridRenderer clearing the canvas doesn't wipe it
            System.Windows.Documents.AdornerLayer adornerLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(MainCanvas);
            CanvasBorderAdorner redBorder = null;
            if (adornerLayer != null)
            {
                redBorder = new CanvasBorderAdorner(MainCanvas);
                adornerLayer.Add(redBorder);
            }

            // Define the logic when a left-click happens
            clickHandler = (s, e) =>
            {
                Point clickPos = e.GetPosition(MainCanvas);
                
                // Unhook events
                MainCanvas.MouseLeftButtonDown -= clickHandler;
                MainCanvas.MouseRightButtonDown -= rightClickHandler;

                // Remove the red border
                if (adornerLayer != null && redBorder != null) adornerLayer.Remove(redBorder);

                var (cols, rows) = _game.getGridDims();
                double cellWidth = MainCanvas.ActualWidth / cols;
                double cellHeight = MainCanvas.ActualHeight / rows;

                int gridX = (int)(clickPos.X / cellWidth);
                int gridY = (int)(clickPos.Y / cellHeight);

                gridX = Math.Max(0, Math.Min(cols - 1, gridX));
                gridY = Math.Max(0, Math.Min(rows - 1, gridY));

                result = (gridX, gridY);
                frame.Continue = false;
            };

            // Define the logic when right-click happens to cancel
            rightClickHandler = (s, e) =>
            {
                // Unhook events
                MainCanvas.MouseLeftButtonDown -= clickHandler;
                MainCanvas.MouseRightButtonDown -= rightClickHandler;
                
                // Remove the red border
                if (adornerLayer != null && redBorder != null) adornerLayer.Remove(redBorder);
                
                // Leave result as (-1, -1) to signify canceled action
                result = (-1, -1);
                frame.Continue = false;
            };

            // Hook them up
            MainCanvas.MouseLeftButtonDown += clickHandler;
            MainCanvas.MouseRightButtonDown += rightClickHandler;

            // This freezes execution of this method without crashing the UI window entirely
            Dispatcher.PushFrame(frame);

            return result;
        }
    }

    // Helper class to draw a border over the Canvas independently of cell children
    public class CanvasBorderAdorner : System.Windows.Documents.Adorner
    {
        public CanvasBorderAdorner(UIElement adornedElement) : base(adornedElement) { }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);

            // Convert the hex string into a SolidColorBrush
            var customColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF5D40");
            var customBrush = new System.Windows.Media.SolidColorBrush(customColor);

            System.Windows.Media.Pen renderPen = new System.Windows.Media.Pen(customBrush, 6);
            drawingContext.DrawRectangle(null, renderPen, adornedElementRect);
        }
    }
}