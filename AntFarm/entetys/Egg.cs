using AntFarm.main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.entetys
{
    internal class Egg : Entity
    {
        public Egg(int id, char Species) : base(id, Species) { }
                public override int Id { get; set; }
        public override char Species { get; set; } = 'E';
        public override string Symbol { get; set; } = "[E]";

        public int hatchTime { get; set; } = 20;
        public void HatchEgg(Game game)
        {
            int y = Position.Item2;
            int x = Position.Item1;
            var worker = new Worker(game.lastEntityId++, 'A');
            game.AddEntityToGameGrid(x, y, worker);
            game.lastEntityId++;
            game.workercount++;
        }


    }
}
