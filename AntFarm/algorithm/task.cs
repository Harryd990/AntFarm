using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.algorithm
{
    public class Task
    {
        public int id;
        public string tasktype;
        public (int, int) targetposition;
        public Task(int id, string tasktype, (int, int) targetposition)
        {
            this.id = id;
            this.tasktype = tasktype;
            this.targetposition = targetposition;
        }
    }
}
