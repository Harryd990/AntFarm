using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AntFarm.Saving
{
    // The TypeDiscriminatorPropertyName defines the key in the JSON ("type": "Ant")
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    
    // Register all your derived classes here
    [JsonDerivedType(typeof(AntSave), typeDiscriminator: "Ant")]
    [JsonDerivedType(typeof(QueenSave), typeDiscriminator: "Queen")]
    [JsonDerivedType(typeof(EggSave), typeDiscriminator: "Egg")]
    [JsonDerivedType(typeof(GenFoodsave), typeDiscriminator: "foodgen")]
    [JsonDerivedType(typeof(FarmSave), typeDiscriminator: "Farm")]
    

    public abstract class EntitySave
    {
        public int x { get; set; }
        public int y { get; set; }
        
        public int id { get; set; }
        public char species { get; set; }
    }

    public class AntSave : EntitySave
    {
        public int food { get; set; }
        public int age { get; set; }
        public int foodcarried { get; set; }
        public taskSaving CurrentTask { get; set; }

    }
    public class QueenSave : AntSave
    {
        public int gestationpeiod { get; set; }
        public int EggGracePeriod { get; set; }
        public bool retreting { get; set; }
    }

    public class EggSave : EntitySave
    {
        public int hatchTime { get; set; }
    }

    public class GenFoodsave : EntitySave
    {
        public int foodcontained { get; set; }
        public int virtFoodContained { get; set; }
        public int maxFoodAmount { get; set; }
    }
  
    public class FarmSave : GenFoodsave
    {
        public int ticksToNextHarvest { get; set; }
    }
}
