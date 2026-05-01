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
               
                if (!File.Exists(filePath))
                {
                    return (null, false);
                }

               
                string jsonText = File.ReadAllText(filePath);

            
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                GameSave? savedData = JsonSerializer.Deserialize<GameSave>(jsonText, jsonOptions);

           
                if (savedData == null)
                {
                    return (null, false);
                }

                
                if (savedData.width <= 0 || savedData.height <= 0)
                {
                    return (null, false);
                }

                
                int expectedCells = savedData.width * savedData.height;
                if (savedData.Cell == null || savedData.Cell.Count != expectedCells)
                {
                    return (null, false);
                }

                
                if (savedData.tick < 0)
                {
                    return (null, false);
                }

               // Create the Game object from the tested Save
                var saveManager = new SaveManager();
               
                Game loadedGame = saveManager.LoadGame(filePath);

                return (loadedGame, true);
            }
            catch (Exception)
            {
                
                return (null, false);
            }
        }
    }
}
