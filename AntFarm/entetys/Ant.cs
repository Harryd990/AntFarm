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

        public virtual int carryingcapacity { get; set; } = 1000;
        public virtual int foodcarried { get; set; } = 0;
        /*
         * commented cos adding food carryied cos idk what else they carry yet
        public virtual List<int> inventory { get; set; } = new List<int>();
        */

        public virtual List<int> path { get; set; } = new List<int>();
        // loop through each position in path and move ant there

        // add it so ants loose food after every tick 
        // --- Added for foodstoregather two-stage task ---
        // The food store position we ultimately must deliver to (set when task assigned)
        public (int, int)? FoodStoreTarget { get; set; } = null;

        // True while the ant is in the phase "go to food source and fill inventory"
        public bool FillingFromSource { get; set; } = false;



        // methord for ants to get food if they have less then 20 food (ungent food gather) 
        public virtual void GatherFood(int amount)
        {
            food += amount;
            if (food > maxfood)
            {
                food = maxfood;
            }
        }
    }
}
