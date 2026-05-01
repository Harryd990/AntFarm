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
        private string _currentStatus = "Welcome to the Ant Farm simulation! the game begins paused (adjust speed slider to begin)";
        private List<string> _actionLogs = new List<string>();

        // Action delegate to cancel the currently active canvas selection
        private Action _cancelCurrentSelection;

        public MainWindow(Game game)
        {
            InitializeComponent();
            _game = game;
            
            // Only run initialization if this is a fresh start (0 ticks progressed)
            if (_game.tick == 0 && _game.totalAntsEver == 0)
            {
                _game.Initialise_Game();
            }
            
            _game.OnSimulationLog += OnGameLogMessage; 

            // Initialize the UI Timer
            _simTimer = new DispatcherTimer();
            _simTimer.Tick += SimTimer_Tick;

            //  Initial rendering
            MainCanvas.Loaded += (s, e) => GridRenderer.Render(_game, MainCanvas);
            MainCanvas.SizeChanged += (s, e) => GridRenderer.Render(_game, MainCanvas);

            // Hook up the Speed Slider
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;

            //  Hook up KeyDown for slider manipulation
            this.KeyDown += MainWindow_KeyDown;

            UpdatesText.Text = _currentStatus;

            // Add the Loaded event for our popup
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
            MessageBoxResult result = MessageBox.Show(
                "have you played this game before", 
                "Tutorial", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            
            if (result == MessageBoxResult.No)
            {
                Window1 tutorialWindow = new Window1();
                tutorialWindow.Show();
            }
        }

        private void RefreshUpdatesText()
        {
            
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


        // used before starting any new tool sequence
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

                // If the user right clicked result is (-1, -1) break loop
                if (gridX == -1 || gridY == -1)
                {
                    OnGameLogMessage("Exited Build farm Tool");
                    break;
                }

                // Check if the cell is underground + open and doesnt overlap an existing building/food
                if (_game.OverGAndOpen((gridX, gridY), _game.GridHeight))
                {
                    AntFarm.algorithm.Task buildtask = new AntFarm.algorithm.Task(_game.queue1.lasttaskid++, "buildfarm", (gridX, gridY));
                    _game.queue1.addtask(buildtask);
                    OnGameLogMessage($"Queued build farm     task at ({gridX}, {gridY})");

                    
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
                
                
                if (gridX == -1 || gridY == -1) 
                {
                    OnGameLogMessage("Exited Build Food Store Tool");
                    break;
                }

                
                if (_game.UnderGAndopen((gridX, gridY), _game.GridHeight))
                {
                    AntFarm.algorithm.Task buildtask = new AntFarm.algorithm.Task(_game.queue1.lasttaskid++, "buildfoodstore", (gridX, gridY));
                    _game.queue1.addtask(buildtask);
                    OnGameLogMessage($"Queued build food store task at ({gridX}, {gridY})");
                    
                   
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
            
            CancelActiveTool();

            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();
                
                
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

            //  popup window
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
        private void SaveGame_Click(object sender, RoutedEventArgs e) 
        {
            if (_game == null) return;

            // Pause the simulation while saving
            double currentSpeed = SpeedSlider.Value;
            SpeedSlider.Value = 0;
            CancelActiveTool();

            // popup window
            Window inputWindow = new Window
            {
                Title = "Save Game",
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            panel.Children.Add(new TextBlock { Text = "Enter a name for your save file:", Margin = new Thickness(0, 0, 0, 10) });

            
            string defaultName = $"Antfarm_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
            TextBox textBox = new TextBox { Text = defaultName, Padding = new Thickness(2) };
            panel.Children.Add(textBox);

            Button submitBtn = new Button { Content = "Save", Width = 80, Margin = new Thickness(0, 15, 0, 0), HorizontalAlignment = HorizontalAlignment.Right };
            submitBtn.Click += (s, args) =>
            {
                string filename = textBox.Text.Trim();
                
                //  the .json extension if they didnt type it
                if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    filename += ".json";
                }

                // Cal save manager
                var saver = new AntFarm.Saving.SaveManager();
                bool saveSuccessful = saver.SaveGame(_game, filename);

                if (!saveSuccessful)
                {
                    MessageBox.Show("Invalid file name. Please try again avoiding characters like *, ?, <, >, |, \\, :, \", /", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // Request input again by stopping the process here and keeping the window open
                    return;
                }

                OnGameLogMessage($"Game logic saved to {filename}");
                inputWindow.Close();

                // Restore previous speed
                SpeedSlider.Value = currentSpeed;
            };
            
            panel.Children.Add(submitBtn);

            inputWindow.Content = panel;
            inputWindow.ShowDialog();
        }
        
        private void GetStats_Click(object sender, RoutedEventArgs e) 
        {
            if (_game == null) return;

            // Get the list of strings from Game.statistics()
            var statsList = _game.statistics();

            
            string statsMsg = string.Join("\n", statsList);

            // Pop  message box
            MessageBox.Show(statsMsg, "Current Game Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Dev Tools
        private void SpawnEntity_Click(object sender, RoutedEventArgs e)
        {
            
            SpeedSlider.Value = 0;

            
            CancelActiveTool();

            // Select Entity Type 
            string selectedType = null;

            Window selectWindow = new Window
            {
                Title = "Select Entity to Spawn",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel selectPanel = new StackPanel { Margin = new Thickness(15) };
            selectPanel.Children.Add(new TextBlock { Text = "Choose an entity type:", Margin = new Thickness(0, 0, 0, 10) });

            ListBox typeListBox = new ListBox { Height = 120 };
            typeListBox.Items.Add("Worker Ant");
            typeListBox.Items.Add("Queen Ant");
            typeListBox.Items.Add("Food");
            typeListBox.Items.Add("Food Store");
            typeListBox.Items.Add("Farm");
            selectPanel.Children.Add(typeListBox);

            Button continueBtn = new Button { Content = "Configure Spawner", Margin = new Thickness(0, 10, 0, 0), Height = 25 };
            continueBtn.Click += (s, args) =>
            {
                if (typeListBox.SelectedItem != null)
                {
                    selectedType = typeListBox.SelectedItem.ToString();
                    selectWindow.Close();
                }
                else
                {
                    MessageBox.Show("Please select an entity type first.");
                }
            };
            selectPanel.Children.Add(continueBtn);

            selectWindow.Content = selectPanel;
            selectWindow.ShowDialog();

            if (selectedType == null) return; 

            // Configure the template properties 
            
            // Temporary storage for configured settings
            int configAge = 0;
            int configFoodLevel = 100;
            int configFoodAmount = 50;
            int configCapacity = 500;
            int configTicksToHarvest = 100;
            bool configuredSuccessfully = false;

            Window configWindow = new Window
            {
                Title = $"Configure {selectedType}",
                Width = 300,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel configPanel = new StackPanel { Margin = new Thickness(15) };

            var textboxes = new Dictionary<string, TextBox>();

            void AddConfigField(string label, int defaultVal)
            {
                configPanel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 5, 0, 2) });
                TextBox tb = new TextBox { Text = defaultVal.ToString(), Padding = new Thickness(2) };
                configPanel.Children.Add(tb);
                textboxes.Add(label, tb);
            }

            // Create fields based on selection
            if (selectedType == "Worker Ant" || selectedType == "Queen Ant")
            {
                AddConfigField("Age", configAge);
                AddConfigField("Food Level", configFoodLevel);
            }
            else if (selectedType == "Food")
            {
                AddConfigField("Food Amount", configFoodAmount);
            }
            else if (selectedType == "Food Store")
            {
                AddConfigField("Initial Food Contained", 0);
                AddConfigField("Capacity", configCapacity);
            }
            else if (selectedType == "Farm")
            {
                AddConfigField("Initial Food Contained", 0);
                AddConfigField("Ticks to Next Harvest", configTicksToHarvest);
            }

            Button saveConfigBtn = new Button { Content = "Start Spawning", Margin = new Thickness(0, 20, 0, 0), Height = 30 };
            saveConfigBtn.Click += (s, args) =>
            {
                try
                {
                    if (textboxes.ContainsKey("Age")) configAge = int.Parse(textboxes["Age"].Text);
                    if (textboxes.ContainsKey("Food Level")) configFoodLevel = int.Parse(textboxes["Food Level"].Text);
                    if (textboxes.ContainsKey("Food Amount")) configFoodAmount = int.Parse(textboxes["Food Amount"].Text);
                    if (textboxes.ContainsKey("Initial Food Contained")) configFoodAmount = int.Parse(textboxes["Initial Food Contained"].Text);
                    if (textboxes.ContainsKey("Capacity")) configCapacity = int.Parse(textboxes["Capacity"].Text);
                    if (textboxes.ContainsKey("Ticks to Next Harvest")) configTicksToHarvest = int.Parse(textboxes["Ticks to Next Harvest"].Text);

                    configuredSuccessfully = true;
                    configWindow.Close();
                }
                catch
                {
                    MessageBox.Show("Please ensure all fields are valid integers.");
                }
            };
            configPanel.Children.Add(saveConfigBtn);
            
            configWindow.Content = new ScrollViewer { Content = configPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            configWindow.ShowDialog();

            if (!configuredSuccessfully) return;

            OnGameLogMessage($"Spawn tool activated for: {selectedType}. Click to spawn, Right-click to exit.");

            // Spawning Loop
            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();

                
                if (gridX == -1 || gridY == -1)
                {
                    OnGameLogMessage("Exited Spawn Tool");
                    break;
                }

                string cellType = _game.GetCellType(gridX, gridY);

                // Check if the cell is valid (not dirt)
                if (cellType.Equals("Dirt", StringComparison.OrdinalIgnoreCase))
                {
                    OnGameLogMessage($"Cannot spawn {selectedType} inside Dirt at ({gridX}, {gridY}). Needs to be Air.");
                    continue;
                }

                // Generate new Entity ID
                int newId = ++_game.lastEntityId;

                // create and configure
                if (selectedType == "Worker Ant")
                {
                    var worker = new Worker(newId, 'A');
                    worker.age = configAge;
                    worker.food = configFoodLevel;
                    worker.Position = (gridX, gridY);
                    _game.AddEntityToGameGrid(gridX, gridY, worker);
                    _game.workercount++;
                    _game.totalAntsEver++;
                }
                else if (selectedType == "Queen Ant")
                {
                    var queen = new Queen(newId, 'Q');
                    queen.age = configAge;
                    queen.food = configFoodLevel;
                    queen.Position = (gridX, gridY);
                    _game.AddEntityToGameGrid(gridX, gridY, queen);
                    _game.totalAntsEver++;
                }
                else if (selectedType == "Food")
                {
                    
                    var food = new Food(newId, 0, 0); 
                    food.Foodrange = (configFoodAmount, configFoodAmount); 
                    food.currentAmount = configFoodAmount;
                    food.virtFoodContained = configFoodAmount;
                    food.Position = (gridX, gridY);
                    _game.AddEntityToGameGrid(gridX, gridY, food);
                }
                else if (selectedType == "Food Store")
                {
                    var store = new FoodStore(newId, 'S');
                    store.capacity = configCapacity;
                    store.foodcontained = configFoodAmount;
                    store.virtFoodContained = configFoodAmount;
                    store.Position = (gridX, gridY);
                    _game.AddEntityToGameGrid(gridX, gridY, store);
                }
                else if (selectedType == "Farm")
                {
                    var farmEnt = new farm(newId, 'R');
                    farmEnt.FoodContained = configFoodAmount;
                    farmEnt.virtFoodContained = configFoodAmount;
                    farmEnt.TickToNextHarvest = configTicksToHarvest;
                    farmEnt.Position = (gridX, gridY);
                    _game.AddEntityToGameGrid(gridX, gridY, farmEnt);
                }

                OnGameLogMessage($"Spawned {selectedType} (ID: {newId}) at ({gridX}, {gridY}).");
                GridRenderer.Render(_game, MainCanvas);
            }
        }
        private void ClearCell_Click(object sender, RoutedEventArgs e) 
        {
            Remover(true);
        }
        private void GetCellDetails_Click(object sender, RoutedEventArgs e)
        {
            
            SpeedSlider.Value = 0;

            
            CancelActiveTool();

            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();

               
                if (gridX == -1 || gridY == -1)
                {
                    OnGameLogMessage("Exited Get Cell Details Tool");
                    break;
                }

                var cell = _game.grid.GetCellAtLocation(gridX, gridY);

                List<string> details = new List<string>
                {
                    $"Cell at ({gridX}, {gridY})",
                    $"Type: {cell.GetType().Name}",
                    $"Entities Present: {cell.Entities.Count}",
                    ""
                };

                foreach (var entity in cell.Entities)
                {
                    details.Add($"- Entity ID={entity.Id}, Type={entity.GetType().Name}");

                    if (entity is AntFarm.entetys.Ant ant)
                    {
                        details.Add($"   -> Food Level: {ant.food}");
                        details.Add($"   -> Age: {ant.age}");
                        details.Add($"   -> Current Task ID: {ant.clamedtaskid}");
                    }
                    else if (entity is AntFarm.entetys.Food food)
                    {
                        details.Add($"   -> Food Amount: {food.currentAmount}");
                    }
                    else if (entity is AntFarm.entetys.FoodStore store)
                    {
                        details.Add($"   -> Contained: {store.foodcontained} / Capacity: {store.capacity}");
                    }
                    else if (entity is AntFarm.entetys.farm farmEntity)
                    {
                        details.Add($"   -> Food Contained: {farmEntity.FoodContained}");
                        details.Add($"   -> Ticks to Harvest: {farmEntity.TickToNextHarvest}");
                        details.Add($"   -> Ant Working: {farmEntity.antWorking} ({farmEntity.antbeenworkingforXticks} ticks)");
                    }
                }

                // Show the details in message box 
                MessageBox.Show(string.Join("\n", details), "Cell Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void EditEntity_Click(object sender, RoutedEventArgs e)
        {
           
            SpeedSlider.Value = 0;

            
            CancelActiveTool();

            while (true)
            {
                var (gridX, gridY) = canvasCellSelect();

                
                if (gridX == -1 || gridY == -1)
                {
                    OnGameLogMessage("Exited Edit Entity Tool");
                    break;
                }

                var cell = _game.grid.GetCellAtLocation(gridX, gridY);
                
                if (cell.Entities.Count == 0)
                {
                    OnGameLogMessage($"No entities found to edit at ({gridX}, {gridY}).");
                    continue; 
                }

                Entity selectedEntity = null;

                //window to select which entity to edit
                Window selectWindow = new Window
                {
                    Title = "Select Entity",
                    Width = 300,
                    Height = 250,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize
                };

                StackPanel selectPanel = new StackPanel { Margin = new Thickness(15) };
                selectPanel.Children.Add(new TextBlock { Text = "Select an entity to edit:", Margin = new Thickness(0, 0, 0, 10) });

                ListBox entityListBox = new ListBox { Height = 120 };
                foreach (var ent in cell.Entities)
                {
                    entityListBox.Items.Add($"{ent.GetType().Name} (ID: {ent.Id})");
                }
                selectPanel.Children.Add(entityListBox);

                Button continueBtn = new Button { Content = "Edit Selected", Margin = new Thickness(0, 10, 0, 0), Height = 25 };
                continueBtn.Click += (s, args) =>
                {
                    if (entityListBox.SelectedIndex >= 0)
                    {
                        selectedEntity = cell.Entities[entityListBox.SelectedIndex];
                        selectWindow.Close();
                    }
                    else
                    {
                        MessageBox.Show("Please select an entity first.");
                    }
                };
                selectPanel.Children.Add(continueBtn);

                selectWindow.Content = selectPanel;
                selectWindow.ShowDialog();

                // If the user selected an entity and clicked continue show edit window
                if (selectedEntity != null)
                {
                    ShowEditEntityDetailsWindow(selectedEntity);
                    GridRenderer.Render(_game, MainCanvas); // Refresh canvas in case visuals changed
                }
            }
        }

        private void ShowEditEntityDetailsWindow(Entity entity)
        {
            Window editWindow = new Window
            {
                Title = $"Edit {entity.GetType().Name} (ID: {entity.Id})",
                Width = 300,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

            // Dictionary to hold functions for applying changes back to the properties 
            // Key: Property name, Value: (TextBox containing new value)
            var intPropertiesToSave = new Dictionary<string, (Action<int>, TextBox)>();

            void AddEditableIntProperty(string propName, int startValue, Action<int> setter)
            {
                panel.Children.Add(new TextBlock { Text = propName, Margin = new Thickness(0, 5, 0, 2) });
                TextBox tb = new TextBox { Text = startValue.ToString(), Padding = new Thickness(2) };
                panel.Children.Add(tb);
                intPropertiesToSave.Add(propName, (setter, tb));
            }
             // may be some issues cos of virtal food contained but as long as the user doesnt set the food to 0 should be fine but even if they do the ant will go away as calcs are done on the actual food stores 
            if (entity is AntFarm.entetys.Ant ant)
            {
                AddEditableIntProperty("Age", ant.age, val => ant.age = val);
                AddEditableIntProperty("Food Level", ant.food, val => ant.food = val);
            }
            else if (entity is AntFarm.entetys.Food food)
            {
                AddEditableIntProperty("Food Amount", food.currentAmount, val => food.currentAmount = val);
                // Note: Intentionally avoiding virtFoodContained
            }
            else if (entity is AntFarm.entetys.FoodStore store)
            {
                AddEditableIntProperty("Food Contained", store.foodcontained, val => store.foodcontained = val);
                AddEditableIntProperty("Capacity", store.capacity, val => store.capacity = val);
            }
            else if (entity is AntFarm.entetys.farm farmEntity)
            {
                AddEditableIntProperty("Food Contained", farmEntity.FoodContained, val => farmEntity.FoodContained = val);
                AddEditableIntProperty("Ticks to Next Harvest", farmEntity.TickToNextHarvest, val => farmEntity.TickToNextHarvest = val);
            }
            else
            {
                panel.Children.Add(new TextBlock { Text = "No editable properties configured for this entity type.", Foreground = System.Windows.Media.Brushes.Gray });
            }

            Button saveBtn = new Button { Content = "Save changes", Margin = new Thickness(0, 20, 0, 0), Height = 30 };
            saveBtn.Click += (s, e) =>
            {
                bool success = true;
                foreach (var kvp in intPropertiesToSave)
                {
                    if (int.TryParse(kvp.Value.Item2.Text, out int result))
                    {
                        kvp.Value.Item1(result);
                    }
                    else
                    {
                        MessageBox.Show($"Invalid input for '{kvp.Key}'. Must be an integer.");
                        success = false;
                    }
                }

                if (success)
                {
                    OnGameLogMessage($"Updated {entity.GetType().Name} properties (ID: {entity.Id})");
                    editWindow.Close();
                }
            };

            panel.Children.Add(saveBtn);
            
            // Wrap in scroll viewer in case it gets tall
            editWindow.Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            editWindow.ShowDialog();
        }

        private void OnGameLogMessage(string message)
        {
            string logLine = $"- {message}";
            _actionLogs.Add(logLine);
            RefreshUpdatesText();

            // remove stuff after 5 secs
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
                    // This removes all entities in the cell (ants all types of food and buildings)
                    var entitiesToRemove = cell.Entities.ToList();
                    foreach (var entity in entitiesToRemove)
                    {
                        cell.RemoveEntity(entity);
                    }

                    // If below groun turn it into dirt
                    if (gridY >= _game.GridHeight / 4)
                    {
                        _game.ReplaceCellAtLocation(gridX, gridY, new Dirt(gridX, gridY));
                    }
                    
                    OnGameLogMessage($"Cleared all entities at ({gridX}, {gridY})");
                }
                else
                {
                    //  remove farms and food stores
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
        
        // this is used in all inputs where user cliks on grid
        public (int, int) canvasCellSelect()
        {
            var frame = new DispatcherFrame();
            (int x, int y) result = (-1, -1);

            System.Windows.Input.MouseButtonEventHandler clickHandler = null;
            System.Windows.Input.MouseButtonEventHandler rightClickHandler = null;
            
            // Add a red border so player knows when they r inputing so the GridRenderer clearing the canvas doesnt wipe it
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

           
            _cancelCurrentSelection = () =>
            {
                result = (-1, -1);
                RemoveHandlersAndBorder();
                frame.Continue = false;
            };

           
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

            
            rightClickHandler = (s, e) =>
            {
                result = (-1, -1);
                RemoveHandlersAndBorder();
                frame.Continue = false;
            };

            
            MainCanvas.PreviewMouseLeftButtonDown += clickHandler;
            MainCanvas.PreviewMouseRightButtonDown += rightClickHandler;

            
            Dispatcher.PushFrame(frame);

            return result;
        }

        // lets user use the right and left arrow keys to change speed
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
            if (e.Key == System.Windows.Input.Key.Left)
            {
                if (SpeedSlider.Value > SpeedSlider.Minimum)
                {
                    
                    SpeedSlider.Value -= 1;
                }
            }
            
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                if (SpeedSlider.Value < SpeedSlider.Maximum)
                {
                    
                    SpeedSlider.Value += 1;
                }
            }
        }
    }

    //class to draw a border over the Canvas independently of cell children
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