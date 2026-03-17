using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm
{
    internal class Dirt : Cell
    {
        public Dirt(int x, int y) : base(x, y)
        {
        }
        public int digprogress { get; set; } = 0;
        public virtual int hardness { get; set; } = 5;
        public override string Symbol => "[@]";
        public override bool IsTraversable => false;

        
    }
}

