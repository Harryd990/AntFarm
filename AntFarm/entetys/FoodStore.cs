using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.entetys
{
   
        internal class FoodStore : Entity
        {
            public FoodStore(int id, char Species) : base(id, Species)
            {

            }
            public override int Id { get; set; }

        
            public int foodcontained { get; set; } = 0;
            public int capacity { get; set; } = 10000;

       
        public override char Species { get; set; } = 'S';
       

        public override (int, int) Position { get; set; }


        // virtual food is used so if a ant has a task to take the food from x it will reserve that food so other ants dont nick it 
        public int virtFoodContained { get; set; } = 0;

        public float fractionoffoodleft()
        {
            return (float)foodcontained / capacity;
        }

        public void addfood(Ant ant)
            {
                foodcontained += ant.foodcarried;
            virtFoodContained += ant.foodcarried;
        }
        
        public void removefood(Ant ant)
            {
                if (foodcontained - ant.foodcarried < 0)
                {
                    foodcontained = 0;
                }
                else
                {
                    foodcontained -= ant.foodcarried;
                }
        }
       


    }
}

