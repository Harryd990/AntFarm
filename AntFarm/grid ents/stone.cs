using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm
{
    internal class stone :Dirt
    {
        // stone is just harder dirt
        public stone(int x, int y) : base(x, y)
        {
        }
        
        public override int hardness { get; set; } = 15;
       
        

    }
}
