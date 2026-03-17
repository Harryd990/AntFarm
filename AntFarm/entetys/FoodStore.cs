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

        // amount of food stored 
            public int foodcontained { get; set; } = 0;
            public int capacity { get; set; } = 10000;

        // symbol and species for this entity
        public override char Species { get; set; } = 'S';
            public override string Symbol { get; set; } = "[S]";

        public override (int, int) Position { get; set; }
        public int virtFoodContained { get; set; } = 0;

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
        // add stuff to make it so food stores can only be made underground + farms too 


    }
}

