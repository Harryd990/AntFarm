using AntFarm.entetys;
using AntFarm.handelers;
using AntFarm.main;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AntFarm
{
    public partial class MainWindow : Window
    {
        private Game _game;
        private DispatcherTimer _simTimer;
        private string _currentStatus = "Simulation ready.";
        private List<string> _actionLogs = new List<string>();

        // Action delegate to cancel the currently active canvas selection
        private Action _cancelCurrentSelection;

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

        private void RefreshUpdatesText()
        {
            // Removed extra linebreaks so logs appear exactly underneath the stats line
            if (_actionLogs.Count > 0)
            {
                UpdatesText.Text = $"{_currentStatus}\n" + string.Join("\n", _actionLogs);
            }
            else
            {
                UpdatesText.Text = _currentStatus;
            }
            
            if (UpdatesText.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        private void SimTimer_Tick(object sender, EventArgs e)
        {
            _game.UpdateTick();
            GridRenderer.Render(_game, MainCanvas);

            // Only update the static stats, and refresh the box
            _currentStatus = $"Simulation Running...\nTick: {_game.tick}\nTasks in Queue: {_game.queue1.tasks.Count}";
            RefreshUpdatesText();
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_simTimer == null) return;

            if (SpeedSlider.Value == 0)
            {
                _simTimer.Stop();
                _currentStatus = $"Simulation Paused (Tick: {_game.tick})\nTasks in Queue: {_game.queue1.tasks.Count}";
                RefreshUpdatesText();
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

        // Use this before starting any new tool sequence
        private void CancelActiveTool()
        {
            if (_cancelCurrentSelection != null)
            {
                _cancelCurrentSelection.Invoke();
                _cancelCurrentSelection = null;
            }
        }

        // Create and Destroy
        private void BuildFarm_Click(object sender, RoutedEventArgs e) 
        {
            // Cancel any old tool first
            CancelActiveTool();

            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();

                // If the user right-clicked, the result is (-1, -1), so we break out of the loop
                if (gridX == -1 || gridY == -1)
                {
                    OnGameLogMessage("Exited Build farm Tool");
                    break;
                }

                // Check if the cell is underground & open, and doesn't overlap an existing building/food
                if (_game.OverGAndOpen((gridX, gridY), _game.GridHeight))
                {
                    AntFarm.algorithm.Task buildtask = new AntFarm.algorithm.Task(_game.queue1.lasttaskid++, "buildfarm", (gridX, gridY));
                    _game.queue1.addtask(buildtask);
                    OnGameLogMessage($"Queued build farm     task at ({gridX}, {gridY})");

                    // Ensure visual update right away
                    GridRenderer.Render(_game, MainCanvas);
                }
                else
                {
                    OnGameLogMessage("Invalid Target: Build Overground without overlapping structures.");
                }
            }
        }
        private void BuildFoodStore_Click(object sender, RoutedEventArgs e)
        {
            // Cancel any old tool first
            CancelActiveTool();

            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();
                
                // If the user right-clicked, the result is (-1, -1), so we break out of the loop
                if (gridX == -1 || gridY == -1) 
                {
                    OnGameLogMessage("Exited Build Food Store Tool");
                    break;
                }

                // Check if the cell is underground & open, and doesn't overlap an existing building/food
                if (_game.UnderGAndopen((gridX, gridY), _game.GridHeight))
                {
                    AntFarm.algorithm.Task buildtask = new AntFarm.algorithm.Task(_game.queue1.lasttaskid++, "buildfoodstore", (gridX, gridY));
                    _game.queue1.addtask(buildtask);
                    OnGameLogMessage($"Queued build food store task at ({gridX}, {gridY})");
                    
                    // Ensure visual update right away
                    GridRenderer.Render(_game, MainCanvas);
                }
                else
                {
                    OnGameLogMessage("Invalid Target: Build underground on non-dirt cells without overlapping structures.");
                }
            }
        }
        
         private void Dig_Click(object sender, RoutedEventArgs e)
        {
            // Cancel any old tool first
            CancelActiveTool();

            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();
                
                // If the user right-clicked, the result is (-1, -1), so we break out of the loop
                if (gridX == -1 || gridY == -1) 
                {
                    OnGameLogMessage("Exited Dig Tool");
                    break;
                }

                string typeName = _game.GetCellType(gridX, gridY);
                
                // Allow both Dirt and stone
                if (typeName == "Dirt" || typeName.Equals("stone", StringComparison.OrdinalIgnoreCase)) 
                {
                    AntFarm.algorithm.Task digtask = new AntFarm.algorithm.Task(_game.queue1.lasttaskid++, "dig", (gridX, gridY));
                    _game.queue1.addtask(digtask);
                    OnGameLogMessage($"Queued dig task at ({gridX}, {gridY})");
                    
                    // Ensure visual update right away
                    GridRenderer.Render(_game, MainCanvas);
                }
                else
                {
                    OnGameLogMessage("Invalid Target: You can only dig through dirt or stone cells.");
                }   
            }
        }
        
        private void RemoveBuilding_Click(object sender, RoutedEventArgs e)
        {
            Remover(false);
        }

        // Settings
        private void IdealPop_Click(object sender, RoutedEventArgs e) 
        {
            if (_game == null) return;

            // Cancel active tools when settings are opened
            CancelActiveTool();

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
                    OnGameLogMessage($"Ideal population updated to {result}");
                    inputWindow.Close();
                }
                else
                {
                    OnGameLogMessage("Invalid Input: Please enter a valid positive number for Ideal Population.");
                }
            };
            panel.Children.Add(submitBtn);

            inputWindow.Content = panel;
            inputWindow.ShowDialog();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            CancelActiveTool();
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
        private void ClearCell_Click(object sender, RoutedEventArgs e) 
        {
            Remover(true);
        }
        private void GetCellDetails_Click(object sender, RoutedEventArgs e) { }
        private void EditEntity_Click(object sender, RoutedEventArgs e) { }

        private void OnGameLogMessage(string message)
        {
            string logLine = $"- {message}";
            _actionLogs.Add(logLine);
            RefreshUpdatesText();

            // Remove this specific line after 5 seconds automatically
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(2000);

                Dispatcher.Invoke(() =>
                {
                    _actionLogs.Remove(logLine);
                    RefreshUpdatesText();
                });
            });
        }
        
        public void Remover(bool Devmode)
        {
            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();

                if (gridX == -1 || gridY == -1)
                {
                    OnGameLogMessage("Exited Remover Tool");
                    break;
                }

                var cell = _game.grid.GetCellAtLocation(gridX, gridY);

                if (Devmode == true)
                {
                    // This removes all entities in the cell (ants, all types of food and buildings)
                    var entitiesToRemove = cell.Entities.ToList();
                    foreach (var entity in entitiesToRemove)
                    {
                        cell.RemoveEntity(entity);
                    }

                    // If below ground, turn it into dirt
                    if (gridY >= _game.GridHeight / 4)
                    {
                        _game.ReplaceCellAtLocation(gridX, gridY, new Dirt(gridX, gridY));
                    }
                    
                    OnGameLogMessage($"Cleared all entities at ({gridX}, {gridY})");
                }
                else
                {
                    // This should just remove farms and food stores
                    var farmEntity = cell.Entities.OfType<farm>().FirstOrDefault();
                    if (farmEntity != null)
                    {
                        cell.RemoveEntity(farmEntity);
                        OnGameLogMessage($"Removed farm at ({gridX}, {gridY})");
                        GridRenderer.Render(_game, MainCanvas);
                        continue;
                    }
                    
                    var storeEntity = cell.Entities.OfType<FoodStore>().FirstOrDefault();
                    if (storeEntity != null)
                    {
                        cell.RemoveEntity(storeEntity);
                        OnGameLogMessage($"Removed food store at ({gridX}, {gridY})");
                        GridRenderer.Render(_game, MainCanvas);
                        continue;
                    }

                    OnGameLogMessage($"No farm or food store found at ({gridX}, {gridY}) to remove.");
                }

                // Force a visual update so the cell clears right away
                GridRenderer.Render(_game, MainCanvas);
            }
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

            void RemoveHandlersAndBorder()
            {
                MainCanvas.PreviewMouseLeftButtonDown -= clickHandler;
                MainCanvas.PreviewMouseRightButtonDown -= rightClickHandler;
                if (adornerLayer != null && redBorder != null) adornerLayer.Remove(redBorder);
                _cancelCurrentSelection = null;
            }

            // Let the UI button click force this nested loop to yield (-1, -1) and break early
            _cancelCurrentSelection = () =>
            {
                result = (-1, -1);
                RemoveHandlersAndBorder();
                frame.Continue = false;
            };

            // Define the logic when a left-click happens
            clickHandler = (s, e) =>
            {
                Point clickPos = e.GetPosition(MainCanvas);

                var (cols, rows) = _game.getGridDims();
                double cellWidth = MainCanvas.ActualWidth / cols;
                double cellHeight = MainCanvas.ActualHeight / rows;

                int gridX = (int)Math.Floor(clickPos.X / cellWidth);
                int gridY = (int)Math.Floor(clickPos.Y / cellHeight);

                gridX = Math.Max(0, Math.Min(cols - 1, gridX));
                gridY = Math.Max(0, Math.Min(rows - 1, gridY));

                result = (gridX, gridY);
                
                RemoveHandlersAndBorder();
                frame.Continue = false;
            };

            // Define the logic when right-click happens to cancel
            rightClickHandler = (s, e) =>
            {
                result = (-1, -1);
                RemoveHandlersAndBorder();
                frame.Continue = false;
            };

            // Hook them up to "Preview" (Tunneling) events so high tick speed rendering clears don't steal the event
            MainCanvas.PreviewMouseLeftButtonDown += clickHandler;
            MainCanvas.PreviewMouseRightButtonDown += rightClickHandler;

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