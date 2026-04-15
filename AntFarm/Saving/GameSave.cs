using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Saving
{
    public class GameSave
    {
       public DateTime SaveTime { get; set; } = DateTime.Now;
        public int height { get; set; }
        public int width { get; set; }
        public int tick { get; set; }
        public List<CellSave> Cell { get; set; } = new();

        public List<EntitySave> Entity { get; set; } = new();
        public GameSave() { }
    }
}
