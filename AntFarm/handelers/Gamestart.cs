using AntFarm.entetys;
using AntFarm.main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AntFarm.handelers
{

    internal class Gamestart
    {

        // class is just validating game start then returning the to main window
        public Gamestart(int gridw, int gridh, int antc, int foodc)
        {

            


        }
        public  (Game, bool)
        {
            int gridArea = gridw * gridh;
            if (!(antc + foodc > gridArea))
            {
                Game game1 = new Game(gridw, gridh, antc, foodc);
        }
}

    }
}
