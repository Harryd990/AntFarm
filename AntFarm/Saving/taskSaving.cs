using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Saving
{
    public class taskSaving
    {
        public int id { get; set; }
        public string taskType { get; set; }
        
        public (int,int) targetpositionX { get; set; }

    }
}
