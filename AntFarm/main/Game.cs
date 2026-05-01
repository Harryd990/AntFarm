using AntFarm.algorithm;
using AntFarm.entetys;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.main
{
   

    public class Game
    {
        
        public event Action<string> OnSimulationLog;

        public Grid grid;
        private Queen queen;
        public queue queue1 = new queue();

        public Game(int width, int height, int startants, int startfood)
        {
            grid = new Grid(width, height);
            queue1 = new queue();
            
            
            StartingFodCount = startfood;
            startingAntCount = startants;
        }
         
        int StartingFodCount { get; set; }
        int startingAntCount { get; set; }
        public int workercount { get; set; } = 0;
        public int GridWidth => grid.width;
        public int GridHeight => grid.height;
        public int tick { get; set; } = 0;
        public int lastEntityId { get; set; } = 0;
        public int QueenFoodCount => queen?.food ?? 0;
        public int IdealPopulation { get; set; } = 10;




        public int totalAntsEver { get; set; } = 0;
        public int totalFoodConsumed { get; set; } = 0;
        public int totalDigsDone { get; set; } = 0;
        public int totalBuildingsMade { get; set; } = 0;






        public (int,int) getGridDims()
        {
            return (grid.width, grid.height);
        }
        public List<string> statistics()
        {
            /*
             * Statistics 
                        Number of ants currently 
                        Number of ants ever 
                        current ant average age
                        Number of food consumed 
                        Number of food in stores 
                        number of buildings made
                        number of digs done 
             */


            List<string> statistics = new List<string> { };

            statistics.Add($"Current Ant Count: {GetAllAnts().Count}");

            statistics.Add($"Total Ants Ever: {totalAntsEver}");

            int sigmage = 0;
            foreach (var ant in GetAllAnts())
            {
                sigmage += ant.age;
            }
            int averageage = GetAllAnts().Count > 0 ? sigmage / GetAllAnts().Count : 0;

            statistics.Add($"Current Ant Average Age: {averageage}");
            statistics.Add($"Total Food Consumed: {totalFoodConsumed}");


            int sumFoodInStores = 0;
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    var foodStores = cell.Entities.OfType<FoodStore>().ToList();
                    foreach (var store in foodStores)
                    {
                        sumFoodInStores += store.foodcontained;
                    }
                }
            }
            statistics.Add($"Total Food in Stores: {sumFoodInStores}");

            statistics.Add($"Total Buildings Made: {totalBuildingsMade}");

            statistics.Add($"Total Digs Done: {totalDigsDone}");



            return statistics;
        }

        public void Initialise_Game()
        {
            
            Random rand = new Random();
            queen = new Queen(0, 'Q');
            AddEntityToGameGrid(grid.width / 2, grid.height / 4 - 1, queen);
            totalAntsEver++;

            for (int i = 2; i <= startingAntCount; i++)
            {
                // adds x workers along the first line of air
                Entity Worker = new Worker(i, 'A'); 
                


                int x = rand.Next(0, grid.width);

                var cell = grid.GetCellAtLocation(x, grid.height / 4 - 1);


                AddEntityToGameGrid(x, grid.height / 4 - 1, Worker);
                
                lastEntityId++;
                workercount++;
                totalAntsEver++;


            }
            for (int i = 1; i <= StartingFodCount; i++)
            {
                // adds x bits of food in the air zone 
                Entity food = new Food(lastEntityId++, 0, 0);
                int x = rand.Next(0, grid.width);
                int y = rand.Next(0, grid.height / 4);
                AddEntityToGameGrid(x, y, food);
                lastEntityId++;
            }

        }
        
        public void UgentHungerCheck() {List<Ant> ants = GetAllAnts();
            foreach (var ant in ants)
            {
                if (ant.food <= 42) // if ant is hungry and is carrying food shift that food from inventory to belly
                    {
                    ant.Transferfood();

                    }
                if ((ant.food <= 40 && ant.clamedtaskid == -1) ) // ant generates own task to get food if food is under 40 and dnt have task
                {
                    
                    ClosestFoodWtaskadd(ant);
                }
                
                if(ant.food <= 20 && ant.clamedtaskid != -1 && ant.Currenttask.tasktype != "gatherfood") // ant is famished cancel current task need scran now
                {
                    
                    
                    if (ant.Currenttask != null)
                    {
                        queue1.addtask(ant.Currenttask);
                    }
                    ClosestFoodWtaskadd(ant);
                }
            }
        }
        public void ReplaceCellAtLocation(int x, int y, Cell newCell)
        {
            grid.ReplaceCellAtLocation(x, y, newCell);
        }
        public void UpdateTick()
        {
            HungerAnts();
            
            UgentHungerCheck();

            ProcessAntMovementAndTasks();

            GeneralTickUpdates();

            if (queen != null)

            {
                // if queen has 60 food and isnt already making babyes and current population is less then the ideal number queen makes egg
                if (checkforUnderGroundSpace() && queen.retreting == false && queen.food >= 60 && queen.EggGracePeriod <= 0 && IdealPopulation > GetAllAnts().Count())
                {
                    algorithm.Task queenTask = new algorithm.Task(queue1.lasttaskid++, "queenretrete", queen.Position);
                    if (queen.clamedtaskid == -1 || queen.Currenttask.tasktype == "wander")
                    {
                        queen.Currenttask = queenTask;
                        queen.retreting = true;
                        queen.clamedtaskid = queenTask.id;
                    }
                    else
                    {
                        queue1.addtask(queen.Currenttask);
                        queen.Currenttask = queenTask;
                        queen.retreting = true;
                        queen.clamedtaskid = queenTask.id;
                    }
                }
            }

            tick++; 
        }

        public void dig(int x, int y)
        {
            if (grid.GetCellAtLocation(x, y) is Dirt)
            {
                Dirt dirtcell = (Dirt)grid.GetCellAtLocation(x, y);
                dirtcell.digprogress++;
                if (dirtcell.digprogress >= dirtcell.hardness)
                {
                    
                    ReplaceCellAtLocation(x, y, new Air(x, y));
                    totalDigsDone++;
                    
                }
            }
            else
            {
                throw new InvalidOperationException("Cell is not dirt");
            }
        }
        public bool checkforUnderGroundSpace()
        {
            // checks if there is underground space to be used for queen and building validsadton 
            // for loop from grid hight /4 to grid height 
            // for loop from 0 to grid width
            // then use GetCellAtLocation to check if its air
            for(int y = grid.height / 4; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    if (cell is Air)
                    {
                        return true;
                        
                    }
                }
            }
            return false;


        }

        public void QueenPromotionCheck()
        {
            var ants = GetAllAnts();
            var aliveQueen = ants.OfType<Queen>().FirstOrDefault();

            if (aliveQueen == null)
            {
                var newQueenWorker = ants.OfType<Worker>().FirstOrDefault(w => w.food > 50);
                if (newQueenWorker != null)
                {
                    // Create Queen and retain all worker attributes
                    queen = new Queen(newQueenWorker.Id, 'Q');
                    queen.Position = newQueenWorker.Position;
                    queen.age = newQueenWorker.age;
                    queen.food = newQueenWorker.food - 20;

                    queen.foodcarried = newQueenWorker.foodcarried;
                    queen.maxfood = newQueenWorker.maxfood;
                    queen.carryingcapacity = newQueenWorker.carryingcapacity;
                    queen.Currenttask = newQueenWorker.Currenttask;
                    queen.clamedtaskid = newQueenWorker.clamedtaskid;
                    queen.path = newQueenWorker.path;
                    queen.FoodStoreTarget = newQueenWorker.FoodStoreTarget;
                    queen.FillingFromSource = newQueenWorker.FillingFromSource;

                   
                    var cell = grid.GetCellAtLocation(newQueenWorker.Position.Item1, newQueenWorker.Position.Item2);
                    cell.RemoveEntity(newQueenWorker);
                    cell.AddEntity(queen);

                   
                }
                else
                {
                    queen = null;
                }
            }
            else
            {
                
                queen = aliveQueen;
            }
        }

        private void UnassignTaskAndReleaseFarm(Ant ant) // if a ant is working on a farm for atlest 30 ticks and a new task (that isnt wander) comes up ant will do that task instead
        {
            if (ant == null) return;

            // If the ant was assigned a farm task clear the farm reservation
            if (ant.Currenttask != null && string.Equals(ant.Currenttask.tasktype, "farmwork", StringComparison.OrdinalIgnoreCase))
            {
                var pos = ant.Currenttask.targetposition;
                if (grid.IsInGridRange(pos.Item1, pos.Item2))
                {
                    var cell = grid.GetCellAtLocation(pos.Item1, pos.Item2);
                    var f = cell.Entities.OfType<farm>().FirstOrDefault();
                    if (f != null) f.antWorking = false;
                }
            }

            
            ant.Currenttask = null;
            ant.clamedtaskid = -1;
            ant.path = null;
            ant.FoodStoreTarget = null;
            ant.FillingFromSource = false;
        }
        public void GeneralTickUpdates()
        {
            if(queen != null)
            {
                queen.EggGracePeriod--;
            }
              
            ProcessEggHatching();
            Check4EmptyStores();
            UpdateFarms();
            QueenPromotionCheck();
            
            /* 
             * decrease egg grace period 
             * decrease egg time to hatch (if 0 hatch it)
             * if food store isnt full create task to get it food
             * tick farms (call over ant if no one is working if ant is working decrease time to harvest if time to harvest is 0 output food)
             */
        }
        public void UpdateFarms()
        {
            // check through grid for farms and call their update method
            for(int x = 0; x < grid.width; x++)
            {
                for(int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    var farms = cell.Entities.OfType<farm>().ToList();
                    foreach(var farm in farms)
                    {
                        farm.TickFarm();
                        bool antAssigned = false;
                        foreach (var ant in GetAllAnts())
                        {
                            if (ant.Currenttask != null &&
                                string.Equals(ant.Currenttask.tasktype, "farmwork", StringComparison.OrdinalIgnoreCase) &&
                                ant.Currenttask.targetposition == (x, y))
                            {
                                antAssigned = true;
                                break;
                            }
                        }

                        // Only add a farmwork task if no ant is assigned and no task is queued
                        if (!farm.antWorking && !antAssigned &&
                            !queue1.tasks.Any(t => string.Equals(t.tasktype, "farmwork", StringComparison.OrdinalIgnoreCase)
                                && t.targetposition == (x, y)))
                        {
                            algorithm.Task foodstoretask = new algorithm.Task(queue1.lasttaskid++, "farmwork", (x, y));
                            queue1.addtask(foodstoretask);
                        }
                    }
                }
            }
        }
        public void Check4EmptyStores()
        {
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    var foodStores = cell.Entities.OfType<FoodStore>().ToList();
                    foreach (var store in foodStores)
                    {
                        if (store.virtFoodContained < store.capacity)
                        {
                            // Count queued tasks targeting this store
                            int queued = queue1.tasks.Count(t =>
                                string.Equals(t.tasktype, "foodstoregather", StringComparison.OrdinalIgnoreCase) &&
                                t.targetposition == (x, y));

                            // Count ants currently assigned to gather for this store
                            int assigned = GetAllAnts().Count(a =>
                                a.Currenttask != null &&
                                string.Equals(a.Currenttask.tasktype, "foodstoregather", StringComparison.OrdinalIgnoreCase) &&
                                a.Currenttask.targetposition == (x, y));

                            // Only add a new task if total (queued + assigned) is less than 5
                            if (queued + assigned < 5)
                            {
                                algorithm.Task foodstoretask = new algorithm.Task(queue1.lasttaskid++, "foodstoregather", (x, y));
                                queue1.addtask(foodstoretask);
                            }
                        }
                    }
                }
            }
        }
        public bool UnderGAndopen((int x, int y) coords, int gridHeight)
        {
            int y = coords.y;
            // Check if y is in the bottom 3/4 of the grid
            if (y < gridHeight / 4)
                return false;

            // Check if the cell is not dirt
            var cellType = GetCellType(coords.x, coords.y);
            if (cellType == "dirt")
                return false;
            if (Check4superimpose(coords) == false)
                return false;
            

            return true;
        }
        public string GetCellType(int x, int y)
        {
            var cell = grid.GetCellAtLocation(x, y);
            return cell.GetType().Name;
        }
        public bool OverGAndOpen((int x, int y) coords, int gridHeight)
        {
            int y = coords.y;
            // Check if y is in the top 3/4 of the grid
            if (y >= gridHeight / 4)
                return false;

            // Check if the cell is not dirt (but this shoould never happen anyway)
            var cellType = GetCellType(coords.x, coords.y);
            if (cellType == "dirt")
                return false;
            if (Check4superimpose(coords) == false)
                return false;

            return true;
        }
       
        public bool Check4superimpose((int,int) cords)
        {
            var cell = grid.GetCellAtLocation(cords.Item1, cords.Item2);
            // check if there is already a valid thing in the position the user is trying to build on if there is then return false and dont build there (not a farm or store or food source)
            if (cell.Entities.Count >= 1)
            {
                if (cell.Entities.OfType<FoodStore>().Any() || cell.Entities.OfType<farm>().Any() || cell.Entities.OfType<Food>().Any())
                {
                    return false;
                }
                else return true;
            }
            else return true;
        }
        public void AddFoodToFoodStore(Ant ant)
        {
            var cell = grid.GetCellAtLocation(ant.Position.Item1, ant.Position.Item2);
            var foodStores = cell.Entities.OfType<FoodStore>().ToList();
            foreach (var store in foodStores)
            {
                store.addfood(ant);
                ant.foodcarried = 0;
                return;
            }

        }


        // egg hatch add up every tick untill = hatch time then add worker to grid at egg pos and remove egg from grid
        public void ProcessEggHatching()
        {
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    var eggs = cell.Entities.OfType<Egg>().ToList();
                    foreach (var egg in eggs)
                    {
                        egg.hatchTime--;
                        if (egg.hatchTime <= 0)
                        {
                            egg.HatchEgg(this);
                            cell.RemoveEntity(egg);
                            
                        }
                    }
                }
            }
        }
        private void ProcessAntMovementAndTasks()
        {
            // 1) Assign queued tasks to idle ants using queue1.getnexttask
            var ants = GetAllAnts();
            foreach (var ant in ants)
            {
                // Only assign a new task if the ant is idle (no current task OR no claimed id (these are almost the same thing but not with farmwork))
                if (ant.Currenttask == null || ant.clamedtaskid == -1)
                {
                    var task = queue1.getnexttask(this, ant);
                    if (task != null)
                    {
                        // Only allow the queen to claim "queenretrete" tasks.
                        // If a non-queen pulled it, re-enqueue the task and skip this ant.
                        if (string.Equals(task.tasktype, "queenretrete", StringComparison.OrdinalIgnoreCase) && !(ant is Queen))
                        {
                            queue1.addtask(task); // put it back for the queen later
                            continue;
                        }

                        // Claim and assign the task to this ant
                        ant.Currenttask = task;
                        ant.clamedtaskid = task.id;
                        ant.path = null;
                      
                    }
                }
            }

            // 2) Move ants along their paths and let idle ants wander
            foreach (var ant in ants)
            {
                if (ant.Currenttask != null)
                {
                    var task = ant.Currenttask;
                    var tx = task.targetposition.Item1;
                    var ty = task.targetposition.Item2;

                    // If this is a dig task and the ant is already adjacent (or on) the target,
                    // do NOT attempt to pathfind — let the ant perform dig work each tick until finished.
                    bool isDigAndAdjacent = task.tasktype.Equals("dig", StringComparison.OrdinalIgnoreCase)
                                            && IsAdjacentOrOn(ant.Position, task.targetposition);

                    if (!isDigAndAdjacent)
                    {
                        // Ensure the ant has a path unless it's already in working position (e.g. adjacent for dig)
                        if (ant.path == null || ant.path.Count == 0)
                        {
                            try
                            {
                                pathfind(ant); // compute path (may target adjacent traversable cell if needed)
                            }
                            catch
                            {
                                // can't reach — release the task so others can try
                                UnassignTaskAndReleaseFarm(ant);
                                continue;
                            }
                        }
                    }

                    // If there is a path move one step per tick
                    if (!isDigAndAdjacent && ant.path != null && ant.path.Count > 0)
                    {
                        int nextIdx = ant.path[0];
                        int nextX = nextIdx % GridWidth;
                        int nextY = nextIdx / GridWidth;

                        // If the next cell became blocked (beacuse of cell clear) try to repath if that fails drop  task
                        var nextCell = grid.GetCellAtLocation(nextX, nextY);
                        if (!nextCell.IsTraversable)
                        {
                            try
                            {
                                pathfind(ant);
                            }
                            catch
                            {
                                // release the ant and any farm reservation
                                UnassignTaskAndReleaseFarm(ant);
                            }
                            continue;
                        }

                        // Move the ant remove from old cell add to new cell update position
                        var oldPos = ant.Position;
                        int oldX = oldPos.Item1;
                        int oldY = oldPos.Item2;
                        var oldCell = grid.GetCellAtLocation(oldX, oldY);
                        oldCell.RemoveEntity(ant);
                        grid.AddEntityToCellLocation(nextX, nextY, ant);
                        ant.Position = (nextX, nextY);

                        // remove the step ant took
                        ant.path.RemoveAt(0);

                        // ant reached the end of the path attempt to work on the task (may start multi-tick dig)
                        if (ant.path.Count == 0)
                        {
                            PerformTaskWork(ant);
                        }
                    }
                    else
                    {
                        // No movement to perform this tick (either because dig-and-adjacent or no path) attempt work
                        PerformTaskWork(ant);
                    }
                }
                else
                {
                    
                    antwander(ant);
                }
            }
        }

        // returns true if pos is same cell or cardinal neighbour of target
        private static bool IsAdjacentOrOn((int, int) pos, (int, int) target)
        {
            int dx = Math.Abs(pos.Item1 - target.Item1);
            int dy = Math.Abs(pos.Item2 - target.Item2);
            return (dx + dy) <= 1;
        }

        
        
            public void FillinvWfood(Ant ant)
            {
            var cell = grid.GetCellAtLocation(ant.Position.Item1, ant.Position.Item2);

            // Try to pick up food from Food entity
            var foods = cell.Entities.OfType<Food>().ToList();
            foreach (var food in foods)
            {
                int spaceleft = ant.carryingcapacity - ant.foodcarried;
                if (spaceleft > 0)
                {
                    if (food.currentAmount <= spaceleft)
                    {
                        ant.foodcarried += food.currentAmount;
                        food.currentAmount = 0;
                        // remove food entity from grid
                        cell.RemoveEntity(food);
                    }
                    else
                    {
                        ant.foodcarried += spaceleft;
                        food.currentAmount -= spaceleft;
                    }
                }
                return;
            }

            // Try to pick up food from Farm entity
            var farms = cell.Entities.OfType<farm>().ToList();
            foreach (var farmEntity in farms)
            {
                int spaceleft = ant.carryingcapacity - ant.foodcarried;
                if (spaceleft > 0 && farmEntity.FoodContained > 0)
                {
                    int take = Math.Min(spaceleft, farmEntity.FoodContained);
                    ant.foodcarried += take;
                    farmEntity.FoodContained -= take;
                    farmEntity.virtFoodContained -= take;
                }
                return;
            }
        }
        
        private void PerformTaskWork(Ant ant)
        {
            if (ant.Currenttask == null) return;

            var task = ant.Currenttask;
            var tx = task.targetposition.Item1;
            var ty = task.targetposition.Item2;

            switch (task.tasktype.ToLowerInvariant())
            {
                case "dig":
                    if (!IsAdjacentOrOn(ant.Position, task.targetposition))
                    {
                        try { pathfind(ant); }
                        catch
                        {
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                        }
                        return;
                    }

                    var targetCell = grid.GetCellAtLocation(tx, ty);
                    if (targetCell is Dirt)
                    {
                        dig(tx, ty);
                        var afterCell = grid.GetCellAtLocation(tx, ty);
                        if (afterCell is Dirt)
                        {
                            ant.path = new List<int>();
                            return;
                        }
                        ant.Currenttask = null;
                        ant.clamedtaskid = -1;
                        ant.path = null;
                        return;
                    }

                    ant.Currenttask = null;
                    ant.clamedtaskid = -1;
                    ant.path = null;
                    return;

                case "gatherfood":
                    if (ant.Position != task.targetposition)
                    {
                        try { pathfind(ant); }
                        catch
                        {
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                        }
                        return;
                    }

                    try
                    {
                        // gather for food 
                        var foodCell = grid.GetCellAtLocation(tx, ty);

                        var foodStore = foodCell.Entities.OfType<FoodStore>().FirstOrDefault(s => s.foodcontained > 0);
                        if (foodStore != null)
                        {
                            int foodNeeded = ant.maxfood - ant.food;
                            if (foodNeeded > 0)
                            {
                                int taken = Math.Min(foodNeeded, foodStore.foodcontained);
                                foodStore.foodcontained -= taken;
                                ant.food += taken;
                            }
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                            return;
                        }

                        // gather for famr
                        var farmStore = foodCell.Entities.OfType<farm>().FirstOrDefault(f => f.FoodContained > 0);
                        if (farmStore != null)
                        {
                            int foodNeeded = ant.maxfood - ant.food;
                            if (foodNeeded > 0)
                            {
                                int taken = Math.Min(foodNeeded, farmStore.FoodContained);
                                farmStore.FoodContained -= taken;
                                ant.food += taken;
                            }
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                            return;
                        }
                      

                        var foodEntity = foodCell.Entities.OfType<Food>().FirstOrDefault();
                        if (foodEntity != null)
                        {
                            int foodNeeded = ant.maxfood - ant.food;
                            if (foodNeeded > 0)
                            {
                                if (foodEntity.currentAmount <= foodNeeded)
                                {
                                    ant.food += foodEntity.currentAmount;
                                    foodEntity.currentAmount = 0;
                                    foodCell.RemoveEntity(foodEntity);
                                    totalFoodConsumed += foodEntity.currentAmount;
                                }
                                else
                                {
                                    ant.food += foodNeeded;
                                    foodEntity.currentAmount -= foodNeeded;
                                    totalFoodConsumed += foodNeeded;
                                }
                            }
                        }
                    }
                    catch
                    {
                        
                    }

                    ant.Currenttask = null;
                    ant.clamedtaskid = -1;
                    ant.path = null;
                    return;



                case "queenretrete":
                    {
                        var q = ant as Queen;
                        if (q == null) return;

                        // Only the queen should ever have this task
                        if (!(ant is Queen)) return;

                        //Find a valid underground open cell (not dirt, not occupied by farms/stores/food)
                        (int x, int y)? FindUndergroundTarget()
                        {
                            var candidates = new List<(int, int)>();
                            for (int yy = grid.height / 4; yy < grid.height; yy++)
                            {
                                for (int xx = 0; xx < grid.width; xx++)
                                {
                                    var cell = grid.GetCellAtLocation(xx, yy);
                                    if (cell is Air &&
                                        !cell.Entities.OfType<FoodStore>().Any() &&
                                        !cell.Entities.OfType<farm>().Any() &&
                                        !cell.Entities.OfType<Food>().Any())
                                    {
                                        candidates.Add((xx, yy));
                                    }
                                }
                            }
                            if (candidates.Count == 0) return null;
                            var rand = new Random();
                            return candidates[rand.Next(candidates.Count)];
                        }

                        // Validate current target cell If invalid, try to pick a new valid underground cell
                        bool currentTargetValid =
                            grid.IsInGridRange(task.targetposition.Item1, task.targetposition.Item2) &&
                            UnderGAndopen(task.targetposition, grid.height) &&
                            !grid.GetCellAtLocation(task.targetposition.Item1, task.targetposition.Item2).Entities.OfType<FoodStore>().Any() &&
                            !grid.GetCellAtLocation(task.targetposition.Item1, task.targetposition.Item2).Entities.OfType<farm>().Any() &&
                            !grid.GetCellAtLocation(task.targetposition.Item1, task.targetposition.Item2).Entities.OfType<Food>().Any();

                        //If the queen is already standing on a valid target dnt pick a new target.
                        if (!currentTargetValid)
                        {
                            var newTarget = FindUndergroundTarget();
                            if (newTarget == null)
                            {
                                //No valid underground place fallback to wander
                                antwander(q);
                                return;
                            }
                            ant.Currenttask.targetposition = newTarget.Value;
                            ant.path = null;
                        }

                        // If not at target pathfind and move
                        if (q.Position != ant.Currenttask.targetposition)
                        {
                            try
                            {
                                pathfind(q);
                            }
                            catch
                            {
                                // cant reach therfore clear task and clear retreating flag
                                ant.Currenttask = null;
                                ant.clamedtaskid = -1;
                                ant.path = null;
                                q.retreting = false;
                                return;
                            }
                            return;
                        }

                        //  begin gestation 
                        if (q.gestationperiod <= 0)
                        {
                            q.gestationperiod = 19; 
                        }

                        // Prevent movement while gestating/laying
                        ant.path = new List<int>(); 
                        
                        q.gestationperiod--;

                        

                        if (q.gestationperiod <= 0)
                        {
                            q.LayEggs(this);
                            q.EggGracePeriod = 30;
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                            q.retreting = false;
                           
                        }
                        return;
                    }

                case "foodstoregather":
                    
                    if (ant.FoodStoreTarget == null)
                        ant.FoodStoreTarget = task.targetposition;

                    //1 find nerest food/farm that has food innit
                    if (!ant.FillingFromSource && ant.foodcarried < ant.carryingcapacity)
                    {
                        Food closestFood = null;
                        farm closestFarm = null;
                        int bestFoodDist = int.MaxValue;
                        int bestFarmDist = int.MaxValue;

                        for (int x = 0; x < grid.width; x++)
                        {
                            for (int y = 0; y < grid.height; y++)
                            {
                                var c = grid.GetCellAtLocation(x, y);
                                foreach (var e in c.Entities)
                                {
                                    if (e is Food f && f.virtFoodContained > 0)
                                    {
                                        int d = Math.Abs(ant.Position.Item1 - x) + Math.Abs(ant.Position.Item2 - y);
                                        if (d < bestFoodDist) { bestFoodDist = d; closestFood = f; }
                                    }
                                    else if (e is farm fm && fm.virtFoodContained > 0)
                                    {
                                        int d = Math.Abs(ant.Position.Item1 - x) + Math.Abs(ant.Position.Item2 - y);
                                        if (d < bestFarmDist) { bestFarmDist = d; closestFarm = fm; }
                                    }
                                }
                            }
                        }

                        if (closestFood == null && closestFarm == null)
                        {
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                            ant.FoodStoreTarget = null;
                            ant.FillingFromSource = false;
                            return;
                        }

                        if (closestFarm != null && (closestFood == null || bestFarmDist < bestFoodDist))
                        {
                            ant.Currenttask.targetposition = closestFarm.Position;
                            closestFarm.virtFoodContained -= Math.Min(ant.carryingcapacity - ant.foodcarried, closestFarm.virtFoodContained);

                        }
                            
                        else if (closestFood != null)
                        {
                            ant.Currenttask.targetposition = closestFood.Position;
                            closestFood.virtFoodContained -= Math.Min(ant.carryingcapacity - ant.foodcarried, closestFood.virtFoodContained);
                        }
                            

                        ant.FillingFromSource = true;
                        ant.path = null;
                        try { pathfind(ant); } catch { }
                        return;
                    }

                    // move to source and fill
                    if (ant.FillingFromSource)
                    {
                        if (ant.Position != ant.Currenttask.targetposition)
                        {
                            try { pathfind(ant); }
                            catch
                            {
                                ant.Currenttask = null;
                                ant.clamedtaskid = -1;
                                ant.path = null;
                                ant.FoodStoreTarget = null;
                                ant.FillingFromSource = false;
                            }
                            return;
                        }

                        FillinvWfood(ant);
                        ant.FillingFromSource = false;

                        if (ant.FoodStoreTarget.HasValue)
                        {
                            ant.Currenttask.targetposition = ant.FoodStoreTarget.Value;
                            ant.path = null;
                            try { pathfind(ant); } catch { }
                            return;
                        }

                        ant.Currenttask = null; ant.clamedtaskid = -1; ant.path = null;
                        return;
                    }

                    //  deliver to store
                    if (ant.FoodStoreTarget.HasValue)
                    {
                        if (ant.Position != ant.FoodStoreTarget.Value)
                        {
                            try { pathfind(ant); }
                            catch
                            {
                                ant.Currenttask = null;
                                ant.clamedtaskid = -1;
                                ant.path = null;
                                ant.FoodStoreTarget = null;
                                ant.FillingFromSource = false;
                            }
                            return;
                        }

                        AddFoodToFoodStore(ant);
                        ant.Currenttask = null;
                        ant.clamedtaskid = -1;
                        ant.path = null;
                        ant.FoodStoreTarget = null;
                        ant.FillingFromSource = false;
                        return;
                    }

                    // fallback
                    ant.Currenttask = null;
                    ant.clamedtaskid = -1;
                    ant.path = null;
                    ant.FoodStoreTarget = null;
                    ant.FillingFromSource = false;
                    return;
                case "farmwork":
                    // Need to be standing on the farm to work If not there path to it.
                    if (ant.Position != task.targetposition)
                    {
                        try
                        {
                            pathfind(ant);
                        }
                        catch
                        {
                            // cant reach the farm  release the task
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                        }
                        return;
                    }

                    // At farm position find the farm entity and mark it as being worked.
                    var farmCell = grid.GetCellAtLocation(tx, ty);
                    var farmEntity = farmCell.Entities.OfType<farm>().FirstOrDefault();
                    if (farmEntity != null)
                    {
                        farmEntity.antWorking = true;
                    }

                    // Check queue for any higher-priority task:
                    // anything that is NOT "wander" and NOT "foodstoregather" is considered higher priority (as in past ants used to sit on the farm untill they died)
                    var urgentTask = queue1.tasks
                        .FirstOrDefault(t => t.id != ant.clamedtaskid
                                             && !string.Equals(t.tasktype, "wander", StringComparison.OrdinalIgnoreCase)
                                             && !string.Equals(t.tasktype, "foodstoregather", StringComparison.OrdinalIgnoreCase));

                    if (urgentTask != null)
                    {
                        // stop working on farm
                        if (farmEntity != null) farmEntity.antWorking = false;

                        // claim the urgent task (remove from queue and assign to this ant)
                        queue1.removetask(urgentTask.id);
                        ant.Currenttask = urgentTask;
                        ant.clamedtaskid = urgentTask.id;
                        ant.path = null; // force path recompute to urgent target
                        return;
                    }

                   
                    ant.path = new List<int>(); 
                    return;

                case "buildfarm": 
                    if (!IsAdjacentOrOn(ant.Position, task.targetposition))
                    {
                        try { pathfind(ant); }
                        catch
                        {
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                        }
                        return;
                    }

                    try
                    {
                        var buildCell = grid.GetCellAtLocation(tx, ty);
                        if (buildCell.IsTraversable) CreateFarm(tx, ty);
                        totalBuildingsMade++;
                    }
                    catch { }

                    ant.Currenttask = null;
                    ant.clamedtaskid = -1;
                    ant.path = null;
                    return;

                case "buildfoodstore": 
                    if (!IsAdjacentOrOn(ant.Position, task.targetposition))
                    {
                        try { pathfind(ant); }
                        catch
                        {
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                        }
                        return;
                    }

                    try
                    {
                        var buildCell = grid.GetCellAtLocation(tx, ty);
                        if (buildCell.IsTraversable) CreateFoodStore(tx, ty);
                        totalBuildingsMade++;
                    }
                    catch { }

                    ant.Currenttask = null;
                    ant.clamedtaskid = -1;
                    ant.path = null;
                    return;

                default:
                    if (!IsAdjacentOrOn(ant.Position, task.targetposition))
                    {
                        try { pathfind(ant); }
                        catch
                        {
                            ant.Currenttask = null;
                            ant.clamedtaskid = -1;
                            ant.path = null;
                        }
                        return;
                    }

                    ant.Currenttask = null;
                    ant.clamedtaskid = -1;
                    ant.path = null;
                    return;
            }
        }

       

        // Gathers all Ant instances currently in the grid helpful allot
        private List<Ant> GetAllAnts()
        {
            List<Ant> ants = new List<Ant>();
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    foreach (var entity in cell.Entities)
                    {
                        if (entity is Ant ant)
                        {
                            ants.Add(ant);
                        }
                    }
                }
            }
            return ants;
        }
        

        public void AddEntityToGameGrid(int x, int y, Entity entity)
        {
            grid.AddEntityToCellLocation(x, y, entity);
        }
        

        public void HungerAnts()
        {
            List<Ant> ants = GetAllAnts();
            foreach (var ant in ants)
            {
                ant.age++;
                ant.food--;
                if (ant.food <= 0)
                {
                    // remove ant from grid cos it died of hunger
                    var pos = ant.Position;
                    var cell = grid.GetCellAtLocation(pos.Item1, pos.Item2);
                    cell.RemoveEntity(ant);
                    workercount--;
                    OnSimulationLog?.Invoke($"An ant has died of hunger at ({pos.Item1},{pos.Item2})");
                }
                if (ant.age >= 500)
                {
                    int rand = new Random().Next(1, 100);
                    if (rand <= 1) // 1% chance of dying from old age
                    {
                        // remove ant from grid
                        var pos = ant.Position;
                        var cell = grid.GetCellAtLocation(pos.Item1, pos.Item2);
                        cell.RemoveEntity(ant);
                        workercount--;
                        OnSimulationLog?.Invoke($"An ant has died of old age at ({pos.Item1},{pos.Item2})");

                    }
                }
                if (ant != null)
                {
                    int rand = new Random().Next(1, 1000);
                    if (rand <= 1) // 0.1% chance of dying from old age
                    {
                        // remove ant from grid
                        var pos = ant.Position;
                        var cell = grid.GetCellAtLocation(pos.Item1, pos.Item2);
                        cell.RemoveEntity(ant);
                        workercount--;
                        OnSimulationLog?.Invoke($"An ant has died of old age at ({pos.Item1},{pos.Item2})");

                    }
                }
                if (ant.age >= 1000)
                {
                    int rand = new Random().Next(1, 100);
                    if (rand <= 5) // 5% chance of dying from old age
                    {
                        // remove ant from grid
                        var pos = ant.Position;
                        var cell = grid.GetCellAtLocation(pos.Item1, pos.Item2);
                        cell.RemoveEntity(ant);
                        workercount--;
                        OnSimulationLog?.Invoke($"An ant has died of old age at ({pos.Item1},{pos.Item2})");

                    }
                }
            }
        }
        
        public void CreateFoodStore(int x, int y)
        {
            if (!grid.IsInGridRange(x, y))
                throw new ArgumentOutOfRangeException(nameof(x), "Position out of grid range.");
           

            
            var fs = new FoodStore(++lastEntityId, 'S');
            AddEntityToGameGrid(x, y, fs);

            
            fs.Position = (x, y);


        }
        public void CreateFarm(int x , int y)
        {
            if (!grid.IsInGridRange(x, y))
                throw new ArgumentOutOfRangeException(nameof(x), "Position out of grid range.");
            var farmentity = new farm(++lastEntityId, 'R');
            AddEntityToGameGrid(x, y, farmentity);
            farmentity.Position = (x, y);
        }


        
        public void antwander(Ant ant) // picks random pos in the "air zone" (for simplicity) ant walks their but claimed task still =-1 so they can be reassigned
        {
            if (queue1.tasks.Count < GetAllAnts().Count && ant.clamedtaskid == -1)
            {
                Random rand = new Random();
                int x = rand.Next(GridWidth);
                int y = rand.Next(0, GridHeight / 4);
                algorithm.Task wander = new algorithm.Task(queue1.lasttaskid++, "wander", (x, y));
                ant.Currenttask = wander;
                ant.clamedtaskid = -1;
            } 
        }



        
        public void ClosestAnt(algorithm.Task task)
        {
            // list of candidate ants
            // - ants with no claimed task
            // - ants currently doing "farmwork" (we allow them to be considered but we will only remove their farm reservation if they are the chosen one)
          
            List<Ant> candidates = new List<Ant>();
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    foreach (var entity in cell.Entities)
                    {
                        if (entity is Ant a)
                        {
                            if (a.clamedtaskid == -1)
                            {
                                candidates.Add(a);
                            }
                            else if (a.Currenttask != null && string.Equals(a.Currenttask.tasktype, "farmwork", StringComparison.OrdinalIgnoreCase))
                            {
                                // Check the farm the ant is assigned to and only include the ant if
                                // the farm reports the ant has been working there for at least 30 ticks so they have the possibility of being reassigned
                                var farmPos = a.Currenttask.targetposition;
                                if (grid.IsInGridRange(farmPos.Item1, farmPos.Item2))
                                {
                                    var farmCell = grid.GetCellAtLocation(farmPos.Item1, farmPos.Item2);
                                    var f = farmCell.Entities.OfType<farm>().FirstOrDefault();
                                    if (f != null && f.antbeenworkingforXticks >= 30)
                                    {
                                        candidates.Add(a);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (candidates.Count == 0)
            {
                throw new Exception("no ants cn task");
            }

            int goalX = task.targetposition.Item1;
            int goalY = task.targetposition.Item2;

            // Find closest candidate
            Ant closestant = null;
            int closestdistance = int.MaxValue;
            foreach (var ant in candidates)
            {
                int distance = Math.Abs(ant.Position.Item1 - goalX) + Math.Abs(ant.Position.Item2 - goalY);
                if (distance < closestdistance)
                {
                    closestdistance = distance;
                    closestant = ant;
                }
            }

            if (closestant == null)
            {
                throw new Exception("no ants cn task");
            }

            // If the chosen ant is currently farming release its farm reservation now (only because it was chosen).
            if (closestant.Currenttask != null && string.Equals(closestant.Currenttask.tasktype, "farmwork", StringComparison.OrdinalIgnoreCase))
            {
                var oldPos = closestant.Currenttask.targetposition;
                if (grid.IsInGridRange(oldPos.Item1, oldPos.Item2))
                {
                    var oldCell = grid.GetCellAtLocation(oldPos.Item1, oldPos.Item2);
                    var oldFarm = oldCell.Entities.OfType<farm>().FirstOrDefault();
                    if (oldFarm != null)
                    {
                        oldFarm.antWorking = false;
                    }
                }

                // Clear the ant's previous farm task state before assigning the new task.
                closestant.Currenttask = null;
                closestant.clamedtaskid = -1;
                closestant.path = null;
                closestant.FoodStoreTarget = null;
                closestant.FillingFromSource = false;
            }

            // Assign the new task
            closestant.Currenttask = task;
            closestant.clamedtaskid = task.id;
            closestant.path = null;

            // Special handling if we just assigned a farmwork task reserve the farm and remove queued duplicates
            if (string.Equals(task.tasktype, "farmwork", StringComparison.OrdinalIgnoreCase))
            {
                if (grid.IsInGridRange(goalX, goalY))
                {
                    var farmCell = grid.GetCellAtLocation(goalX, goalY);
                    var f = farmCell.Entities.OfType<farm>().FirstOrDefault();
                    if (f != null) f.antWorking = true;
                }

                // Remove any other queued farmwork tasks for the same position to avoid duplicate assignments
                queue1.tasks.RemoveAll(t =>
                    t.id != task.id &&
                    string.Equals(t.tasktype, "farmwork", StringComparison.OrdinalIgnoreCase) &&
                    t.targetposition == task.targetposition);
            }
        }
        // methord to find closes food thing (store or just food) to ant
        public void ClosestFoodWtaskadd(Ant ant)
        {
            // ants go to food stored b4 they try farm food if not then natural food 
            
            List<FoodStore> foodStores = new List<FoodStore>();
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    foreach (var entity in cell.Entities)
                    {
                        if (entity is FoodStore store && store.virtFoodContained > 0)
                        {
                            foodStores.Add(store);
                        }
                    }
                }
            }
            if (foodStores.Count > 0)
            {
                int closestdistance = int.MaxValue;
                FoodStore closestStore = null;
                foreach (var store in foodStores)
                {
                    int distance = Math.Abs(ant.Position.Item1 - store.Position.Item1) + Math.Abs(ant.Position.Item2 - store.Position.Item2);
                    if (distance < closestdistance)
                    {
                        closestdistance = distance;
                        closestStore = store;
                    }
                }
                if (closestStore != null)
                {
                    algorithm.Task foodtask = new algorithm.Task(queue1.lasttaskid++, "gatherfood", closestStore.Position);
                    closestStore.virtFoodContained -= Math.Min(ant.maxfood - ant.food, closestStore.virtFoodContained);
                    ant.Currenttask = foodtask;
                    ant.clamedtaskid = foodtask.id;
                    return;
                }
            }

            //  If no FoodStore with food try to find the closest Farm with food 
            List<farm> foodFarms = new List<farm>();
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    foreach (var entity in cell.Entities)
                    {
                        if (entity is farm f && f.virtFoodContained > 0)
                        {
                            foodFarms.Add(f);
                        }
                    }
                }
            }
            if (foodFarms.Count > 0)
            {
                int closestdistanceFarm = int.MaxValue;
                farm closestFarm = null;
                foreach (var f in foodFarms)
                {
                    int distance = Math.Abs(ant.Position.Item1 - f.Position.Item1) + Math.Abs(ant.Position.Item2 - f.Position.Item2);
                    if (distance < closestdistanceFarm)
                    {
                        closestdistanceFarm = distance;
                        closestFarm = f;
                    }
                }
                if (closestFarm != null)
                {
                    algorithm.Task foodtask = new algorithm.Task(queue1.lasttaskid++, "gatherfood", closestFarm.Position);
                    closestFarm.virtFoodContained -= Math.Min(ant.maxfood - ant.food, closestFarm.virtFoodContained);
                    ant.Currenttask = foodtask;
                    ant.clamedtaskid = foodtask.id;
                    return;
                }
            }
            

            // 3 If no FoodStore and no Farm with food fall back to closest natural Food entity
            List<Food> foodnat = new List<Food>();
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    var cell = grid.GetCellAtLocation(x, y);
                    foreach (var entity in cell.Entities)
                    {
                        if (entity is Food food && food.virtFoodContained >= Math.Min(ant.maxfood - ant.food, food.virtFoodContained))
                        {
                            foodnat.Add(food);
                        }
                    }
                }
            }
           
            int closestdistance2 = int.MaxValue;
            Food closestfood = null;
            foreach (var food in foodnat)
            {
                int distance = Math.Abs(ant.Position.Item1 - food.Position.Item1) + Math.Abs(ant.Position.Item2 - food.Position.Item2);
                if (distance < closestdistance2)
                {
                    closestdistance2 = distance;
                    closestfood = food;
                }
            }
            if (closestfood != null)
            {
                algorithm.Task foodtask = new algorithm.Task(queue1.lasttaskid++, "gatherfood", closestfood.Position);
                closestfood.virtFoodContained -= Math.Min(ant.maxfood - ant.food, closestfood.virtFoodContained);
                ant.Currenttask = foodtask;
                ant.clamedtaskid = foodtask.id;
            }
            
        }

        public void pathfind(Ant ant)
        {

            // check if start and end are the same
            // check area around ant to see if its full boxed
            // if any of these then thwrow exception
           

            // return path into ant path list then ant moves along it each tick
            if (ant == null) throw new ArgumentNullException(nameof(ant));
            if (ant.Currenttask == null) throw new InvalidOperationException("Ant has no task to pathfind to.");

            var start = ant.Position;
            var end = ant.Currenttask.targetposition;

            int startX = start.Item1;
            int startY = start.Item2;
            int endX = end.Item1;
            int endY = end.Item2;

            if (!grid.IsInGridRange(startX, startY))
                throw new ArgumentOutOfRangeException("Ant start position out of grid range.");
            if (!grid.IsInGridRange(endX, endY))
                throw new ArgumentOutOfRangeException("Task target position out of grid range.");

            int startIdx = startY * GridWidth + startX;
            int goalX = endX;
            int goalY = endY;

            // if goal cell ismnt traversable find closest adjacent traversable cell 
            var goalCell = grid.GetCellAtLocation(endX, endY);
            if (!goalCell.IsTraversable)
            {
                // look for adjacent traversable cell
                (int x, int y)? best = null;
                int bestDist = int.MaxValue;
                // check 4 directions   
                (int dx, int dy)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
                foreach (var d in dirs)
                {
                    int nx = endX + d.dx;
                    int ny = endY + d.dy;
                    // skip positions outside the grid
                    if (!grid.IsInGridRange(nx, ny)) continue;
                    var nc = grid.GetCellAtLocation(nx, ny);
                    // only consider if ant can go there 
                    if (!nc.IsTraversable) continue;
                    if (nc.IsTraversable)
                    {
                        // use manhattan distance to find closest (dm cos canot move diagonally) (ts is the heuristc)    
                        int manhattan = Math.Abs(nx - startX) + Math.Abs(ny - startY);
                        if (manhattan < bestDist)
                        {
                            bestDist = manhattan;
                            best = (nx, ny);
                        }
                    }
                }
                if (best.HasValue)
                {
                    // set the goal to the chosen cell to get to (for food it its the cell for dig cell next to it)
                    goalX = best.Value.x;
                    goalY = best.Value.y;
                }
                else
                {
                    
                    throw new InvalidOperationException("No reachable traversable cell adjacent to target.");
                }
            }

            int goalIdx = goalY * GridWidth + goalX;

            // If already at goal empty path
            if (startX == goalX && startY == goalY)
            {
                ant.path = new List<int>(); 
                return;
            }
            
            int n = GridWidth * GridHeight;
            var dist = new int[n];
            var parent = new int[n];
            var visited = new bool[n];
            for (int i = 0; i < n; i++)
            {
                dist[i] = int.MaxValue;
                parent[i] = -1;
            }

            var pq = new PriorityQueue<int, int>();
            dist[startIdx] = 0;
            pq.Enqueue(startIdx, 0);

            (int dx, int dy)[] neighbors = { (1, 0), (-1, 0), (0, 1), (0, -1) };

            while (pq.Count > 0)
            {
                int u = pq.Dequeue();
                if (visited[u]) continue;
                visited[u] = true;

                if (u == goalIdx) break;

                int ux = u % GridWidth;
                int uy = u / GridWidth;

                foreach (var d in neighbors)
                {
                    int vx = ux + d.dx;
                    int vy = uy + d.dy;
                    if (!grid.IsInGridRange(vx, vy)) continue;
                    int v = vy * GridWidth + vx;

                    // allow traversal if cell is traversable
                    var vcell = grid.GetCellAtLocation(vx, vy);
                    if (!vcell.IsTraversable) continue;

                    int alt = dist[u] + 1; 
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        parent[v] = u;
                        pq.Enqueue(v, alt);
                    }
                }
            }

            if (parent[goalIdx] == -1)
            {
                // add so that if this happens task is attempted again at back of quque or passed onto dif ant 
                throw new InvalidOperationException("No path found from ant to task.");
            }

            // reconstruct path (from start to goal)
            var rev = new List<int>();
            for (int at = goalIdx; at != -1; at = parent[at])
            {
                rev.Add(at);
            }
            rev.Reverse();

            // Remove the first element if it is the start cell, so ant.path contains steps to take (next cell first)
            if (rev.Count > 0 && rev[0] == startIdx)
            {
                rev.RemoveAt(0);
            }

            ant.path = rev;
        }


    }

}
