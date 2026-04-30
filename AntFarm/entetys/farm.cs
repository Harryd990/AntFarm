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
        
       

        public int virtFoodContained { get; set; } = 0;
        public int FoodContained { get; set; } = 0;

        public int antbeenworkingforXticks { get; set; } = 0;
        public int TickToNextHarvest { get; set; } = 10;
        public bool antWorking { get; set; } = false;

        public int maxFoodAmount { get; set; } = 10000;
        public float fractionOfFoodLeft()
        {
            return (float)FoodContained / maxFoodAmount;
        }

        public void HarvestFarm()
        {
            if (antWorking && FoodContained <= 9000)
            {
                FoodContained += 1000;
                virtFoodContained += 1000;
                TickToNextHarvest = 10;
            }
            if(antWorking && FoodContained > 9000)
            {
                FoodContained = maxFoodAmount;
                virtFoodContained = maxFoodAmount;
                TickToNextHarvest = 10;
            }

            TickToNextHarvest = 10;
         
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
