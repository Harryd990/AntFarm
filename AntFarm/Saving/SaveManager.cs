using System;
using System.IO;
using System.Text.Json;
using AntFarm.entetys;
using AntFarm.main;

namespace AntFarm.Saving
{
    public class SaveManager
    {
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Formats the JSON to be readable
        };

        public void SaveGame(Game game, string filePath = "autosave.json")
        {
            // Always target the Documents folder
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string saveDirectory = Path.Combine(documentsFolder, "AntFarmSaves");
            
            // Ensure the directory exists
            Directory.CreateDirectory(saveDirectory); 

            // If the UI passes just a filename (e.g., "save1.json"), we combine it with the Document's save directory.
            
            string safeFileName = string.IsNullOrWhiteSpace(filePath) ? "autosave.json" : Path.GetFileName(filePath);
            
            string fullPath = Path.Combine(saveDirectory, safeFileName);

            
            // innitalise master game same
            var gameSave = new GameSave
            {
                SaveTime = DateTime.Now,
                width = game.GridWidth,
                height = game.GridHeight,
                tick = game.tick
            };

            //Loop through the grid
            for (int x = 0; x < game.GridWidth; x++)
            {
                for (int y = 0; y < game.GridHeight; y++)
                {
                    var cell = game.grid.GetCellAtLocation(x, y);

                    // Add cell data
                    gameSave.Cell.Add(new CellSave
                    {
                        X = x,
                        Y = y,
                        CellType = cell.GetType().Name
                    });

                    // Process Entities on that cell
                    foreach (var entity in cell.Entities)
                    {
                        EntitySave? saveObj = null;

                        if (entity is Queen queen)
                        {
                            saveObj = new QueenSave
                            {
                                id = queen.Id, species = queen.Species, x = x, y = y,
                                food = queen.food, age = queen.age, foodcarried = queen.foodcarried,
                                gestationpeiod = queen.gestationperiod, EggGracePeriod = queen.EggGracePeriod, retreting = queen.retreting
                            };
                        }
                        else if (entity is Worker worker)
                        {
                            saveObj = new AntSave
                            {
                                id = worker.Id, species = worker.Species, x = x, y = y,
                                food = worker.food, age = worker.age, foodcarried = worker.foodcarried
                            };
                        }
                        else if (entity is Egg egg)
                        {
                            saveObj = new EggSave
                            {
                                id = egg.Id, species = egg.Species, x = x, y = y,
                                hatchTime = egg.hatchTime
                            };
                        }
                        else if (entity is Food food)
                        {
                            saveObj = new GenFoodsave
                            {
                                id = food.Id, species = food.Species, x = x, y = y,
                                foodcontained = food.currentAmount, maxFoodAmount = food.maxFoodAmount, virtFoodContained = food.virtFoodContained
                            };
                        }
                        else if (entity is FoodStore store)
                        {
                            saveObj = new GenFoodsave
                            {
                                id = store.Id, species = store.Species, x = x, y = y,
                                foodcontained = store.foodcontained, maxFoodAmount = store.capacity, virtFoodContained = store.virtFoodContained
                            };
                        }
                        else if (entity is farm f)
                        {
                            saveObj = new FarmSave
                            {
                                id = f.Id, species = f.Species, x = x, y = y,
                                foodcontained = f.FoodContained,
                                maxFoodAmount = f.maxFoodAmount,
                                ticksToNextHarvest = f.TickToNextHarvest
                            };
                        }

                        if (saveObj != null)
                        {
                            gameSave.Entity.Add(saveObj);
                        }
                    }
                }
            }

           
            try
            {
                string jsonString = JsonSerializer.Serialize(gameSave, jsonOptions);
                File.WriteAllText(fullPath, jsonString);
                
            }
            catch (Exception ex)
            {
               
            }
        }

        public Game LoadGame(string filePath)
        {
            string jsonText = File.ReadAllText(filePath);
            GameSave? savedData = JsonSerializer.Deserialize<GameSave>(jsonText, jsonOptions);

            if (savedData == null)
            {
                throw new Exception("Save data could not be parsed.");
            }
            
           
            Game restoredGame = new Game(savedData.width, savedData.height, 0, 0);

            
            for (int x = 0; x < savedData.width; x++)
            {
                for (int y = 0; y < savedData.height; y++)
                {
                    restoredGame.grid.GetCellAtLocation(x, y).Entities.Clear();
                }
            }

            
            foreach (var cellSave in savedData.Cell)
            {
                Cell newCell = cellSave.CellType.ToLower() switch
                {
                    "air" => new Air(cellSave.X, cellSave.Y),
                    "stone" => new stone(cellSave.X, cellSave.Y),
                    "dirt" => new Dirt(cellSave.X, cellSave.Y),
                    _ => new Dirt(cellSave.X, cellSave.Y)
                };
                
                restoredGame.ReplaceCellAtLocation(cellSave.X, cellSave.Y, newCell);
            }
            

            restoredGame.tick = savedData.tick;

            // Recreate entities
            int maxIdFound = 0;

            foreach (var entSave in savedData.Entity)
            {
                if (entSave.id > maxIdFound)
                {
                    maxIdFound = entSave.id;
                }

                Entity? newEntity = null;

                if (entSave is QueenSave qs)
                {
                    var queen = new Queen(qs.id, qs.species)
                    {
                        food = qs.food,
                        age = qs.age,
                        foodcarried = qs.foodcarried,
                        gestationperiod = qs.gestationpeiod,
                        EggGracePeriod = qs.EggGracePeriod,
                        retreting = qs.retreting 
                    };
                    newEntity = queen;
                    restoredGame.totalAntsEver++;
                }
                else if (entSave is AntSave ws)
                {
                    var worker = new Worker(ws.id, ws.species)
                    {
                        food = ws.food,
                        age = ws.age,
                        foodcarried = ws.foodcarried
                    };
                    newEntity = worker;
                    restoredGame.workercount++;
                    restoredGame.totalAntsEver++;
                }
                else if (entSave is EggSave es)
                {
                    var egg = new Egg(es.id, es.species)
                    {
                        hatchTime = es.hatchTime
                    };
                    newEntity = egg;
                }
                else if (entSave is FarmSave fs) 
                {
                    if (fs.species == 'R') 
                    {
                        var farmEnt = new farm(fs.id, fs.species)
                        {
                            FoodContained = fs.foodcontained,
                            maxFoodAmount = fs.maxFoodAmount,
                            virtFoodContained = fs.virtFoodContained, 
                            TickToNextHarvest = fs.ticksToNextHarvest
                        };
                        newEntity = farmEnt;
                        restoredGame.totalBuildingsMade++;
                    }
                }
                else if (entSave is GenFoodsave gfs)
                {
                    if (gfs.species == 'F')
                    {
                        var food = new Food(gfs.id, gfs.x, gfs.y)
                        {
                            currentAmount = gfs.foodcontained,
                            maxFoodAmount = gfs.maxFoodAmount,
                            virtFoodContained = gfs.virtFoodContained
                        };
                        newEntity = food;
                    }
                    else if (gfs.species == 'S')
                    {
                        var store = new FoodStore(gfs.id, gfs.species)
                        {
                            foodcontained = gfs.foodcontained,
                            capacity = gfs.maxFoodAmount,
                            virtFoodContained = gfs.virtFoodContained
                        };
                        newEntity = store;
                        restoredGame.totalBuildingsMade++;
                    }
                }

                // Place the entity inside the correct grid slot 
                if (newEntity != null)
                {
                    restoredGame.AddEntityToGameGrid(entSave.x, entSave.y, newEntity);
                }
            }

            // Sync the ID generator in Game so new ants don't overlap IDs with existing ones
            restoredGame.lastEntityId = maxIdFound + 1;

            return restoredGame;
        }
    }
}