using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.entetys
{
    internal class farm : Entity
    {
        public farm(int id, char Species) : base(id, Species) { }
        public override int Id { get; set; }
        public override char Species { get; set; } = 'R';
        public override string Symbol { get; set; } = "[R]";
        // farm generates food over time every 10 ticks 100 food is made costs 10 food to create a farm
        // chane gathe foods to sore thing to haul food so can be used on farms too 
        // ant needs to work in farm for it t make food ( farm makes enough food for 10 ants)

        // add virtual food contained (so tasks can claim food from farm without doubbeling up)

        public int virtFoodContained { get; set; } = 0;
        public int FoodContained { get; set; } = 0;

        public int antbeenworkingforXticks { get; set; } = 0;
        public int TickToNextHarvest { get; set; } = 10;
        public bool antWorking { get; set; } = false;
        
        public void HarvestFarm()
        {
            if (antWorking)
            {
                FoodContained += 1000;
                virtFoodContained += 1000;
                TickToNextHarvest = 10;
            }

            TickToNextHarvest = 10;
            // ant been working stuff so ants will work for atleast a while on each farm before dipping strait away 

            // add virtual food contained (so tasks can claim food from farm without doubbeling up)

        }
        public void TickFarm()
        {
            if (TickToNextHarvest > 0 && antWorking)
            {
                TickToNextHarvest--;
                antbeenworkingforXticks++;
            }
            if (TickToNextHarvest == 0)
            {
                HarvestFarm();
                antbeenworkingforXticks++;
            }
            if(antWorking ==false)
            {
                antbeenworkingforXticks = 0;
            }
        }


    }
}
