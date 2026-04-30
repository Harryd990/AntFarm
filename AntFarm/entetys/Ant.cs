using AntFarm.algorithm;
using AntFarm.main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.entetys
{
    public abstract class Ant : Entity
    {
        public Ant(int id, char Species) : base(id, Species)
        { 

        }
        public int clamedtaskid { get; set; } = -1;
        public algorithm.Task Currenttask { get; set; }
        // -1 means no class claimed
        // add stuff for queue so once ant has clamed a task it wont claim another till done
        public override int Id { get; set; }
        public override char Species { get; set; } = 'N';
        public override string Symbol { get; set; } = "[N]";

        // added food to start at max food could change but idk
        public virtual int food { get; set; } = 20;
        public virtual int maxfood { get; set; } = 100;

        public virtual int age { get; set; } = 0;

        public virtual int carryingcapacity { get; set; } = 1000;
        public virtual int foodcarried { get; set; } = 0;
       

        public virtual List<int> path { get; set; } = new List<int>();
       
        public (int, int)? FoodStoreTarget { get; set; } = null;

        
        public bool FillingFromSource { get; set; } = false;


        public void Transferfood()
        {
            int fdneedtotrans = 0;
            fdneedtotrans = maxfood - food;
            carryingcapacity = carryingcapacity - fdneedtotrans;
            food = food + fdneedtotrans;
        }
        
        
    }
}
