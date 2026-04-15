using AntFarm.entetys;
using AntFarm.main;
using AntFarm.Saving;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace AntFarm.handelers
{
    public class Gamestart
    {
        // class is just validating game start then returning the to main window
        public static (Game, bool) HandleNewGame(Game PreviousGame, int gridw, int gridh, int antc, int foodc)
        {
            int gridArea = gridw * gridh;

            if (!(antc + foodc > gridArea))
            {
                return (new Game(gridw, gridh, antc, foodc), true);
            }
            else
            {
                return (PreviousGame, false);
            }
        }

        // Method for loading games with basic corruption checking
        public static (Game? loadedGame, bool isValid) HandleLoadGame(string filePath)
        {
            try
            {
                // 1. Check if the file actually exists
                if (!File.Exists(filePath))
                {
                    return (null, false);
                }

                // 2. Read the JSON text to deserialize into GameSave directly to test validity
                string jsonText = File.ReadAllText(filePath);

                // Allow polymorphism deserialization (same as saving logic)
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                GameSave? savedData = JsonSerializer.Deserialize<GameSave>(jsonText, jsonOptions);

                // 3. Null check
                if (savedData == null)
                {
                    return (null, false);
                }

                // --------- Basic Corruption Checks --------- //

                // Check A: Dimensions must be positive integers
                if (savedData.width <= 0 || savedData.height <= 0)
                {
                    return (null, false);
                }

                // Check B: The number of saved cells must exactly match the defined grid matrix (width * height)
                // This checks if the cell list is corrupted, malformed, missing, or expanded.
                int expectedCells = savedData.width * savedData.height;
                if (savedData.Cell == null || savedData.Cell.Count != expectedCells)
                {
                    return (null, false);
                }

                // Check C: Negative Tick value (Time doesn't go backwards)
                if (savedData.tick < 0)
                {
                    return (null, false);
                }

                // IF ALL CHECKS PASS: Create the Game object from the tested Save
                var saveManager = new SaveManager();
               
                Game loadedGame = saveManager.LoadGame(filePath);

                return (loadedGame, true);
            }
            catch (Exception)
            {
                // File couldn't be loaded (JSON parsing fail, corrupted types, unauthorized access, etc.)
                return (null, false);
            }
        }
    }
}
