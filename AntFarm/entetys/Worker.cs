using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.entetys
{
    internal class Worker : Ant
    {
        public Worker(int id, char Species) : base(id, Species)
        {

        }
        public override char Species { get; set; } = 'A';
        public override string Symbol { get; set; } = "[A]";

        
       
    }
}
