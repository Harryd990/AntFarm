using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm
{
    public class Air : Cell
    {
        public Air(int x, int y) : base(x, y)
        {

        }
        
       
        public override bool IsTraversable => true;

        
        
    }

}
    

