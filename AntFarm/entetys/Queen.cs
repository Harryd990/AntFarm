using AntFarm.main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.entetys
{
    internal class Queen : Ant 
    {
        public Queen(int id, char Species) : base(id, Species)
        {

        }
        public override char Species { get; set; } = 'Q';
        public override string Symbol { get; set; } = "[Q]";
        public int gestationperiod { get; set; } = 19;
        public int EggGracePeriod { get; set; } = 100;

        public bool retreting { get; set; } = false;

        // makes new egg at queen position
        public void LayEggs(Game game)
        {
            int y = Position.Item2;
            int x = Position.Item1;
            var egg = new Egg(game.lastEntityId++, 'E');
            game.AddEntityToGameGrid(x, y, egg);

        }
    }
}
