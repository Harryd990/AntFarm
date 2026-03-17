using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.entetys
{
    public abstract class Entity
    {
        public abstract int Id { get; set; }
        public abstract char Species { get; set; }

        public abstract string Symbol { get; set; }

        public virtual (int,int) Position{ get; set; }

    public Entity(int id, char Species)
        {
            Id = id;
            this.Species = Species;

        }
    }
    
}
