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
       
        public override int Id { get; set; }
        public override char Species { get; set; } = 'N';
       
        public virtual int food { get; set; } = 20;
        public virtual int maxfood { get; set; } = 100;

        public virtual int age { get; set; } = 0;

        public virtual int carryingcapacity { get; set; } = 5000;
        public virtual int foodcarried { get; set; } = 0;
       

        public virtual List<int> path { get; set; } = new List<int>();
       
        public (int, int)? FoodStoreTarget { get; set; } = null;

        
        public bool FillingFromSource { get; set; } = false;


        public void Transferfood()
        {
            int fdneedtotrans = 0;
            fdneedtotrans = maxfood - food;
            if(foodcarried >0 && foodcarried >= fdneedtotrans)
            {
                foodcarried = foodcarried - fdneedtotrans;
                food = food + fdneedtotrans;
            }
            if(foodcarried > 0 && foodcarried < fdneedtotrans)
            {
                food = food + foodcarried;
                foodcarried = 0;
            }

        }
        
        
    }
}
