using AntFarm.entetys;
using AntFarm.main;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
    }
}
