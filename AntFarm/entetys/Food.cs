using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.entetys
{
    internal class Food : Entity
    {
        public Food(int id, int x, int y) : base(id, 'F')
        {
            Random random = new Random();
            int temp = 0;
            
            Foodrange = (25, random.Next(1000, 10000));
            temp = random.Next(Foodrange.Item1, Foodrange.Item2 + 1);
            virtFoodContained = temp;
            currentAmount = temp;
            maxFoodAmount = temp;

            Id = Id;
        }
        public int maxFoodAmount { get; set; }
        public override int Id { get; set; }
        public override char Species { get; set; } = 'F';
        


        public int virtFoodContained { get; set; } = 0;

        public (int, int) Foodrange { get; set; }
        public int currentAmount { get; set; }

        public float fractionOfFoodLeft()
        {
            return (float)currentAmount / maxFoodAmount;
        }
    }
}
