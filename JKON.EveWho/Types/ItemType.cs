using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JKON.EveWho.Types
{
    public class ItemType
    {
        [DataMember(Name = "type_id")]
        public long Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "published")]
        public bool Published { get; set; }

        [DataMember(Name = "group_id")]
        public long Group_Id { get; set; }

        [DataMember(Name = "radius")]
        public long Radius { get; set; }

        [DataMember(Name = "volume")]
        public long Volume { get; set; }

        [DataMember(Name = "capacity")]
        public long Capacity { get; set; }

        [DataMember(Name = "portion_size")]
        public long Portion_Size { get; set; }

        [DataMember(Name = "mass")]
        public float Mass { get; set; }

        [DataMember(Name = "graphic_id")]
        public long Graphic_Id { get; set; }

        [DataMember(Name = "dogma_attributes")]
        public Dogma_Attributes[] Dogma_Attributes { get; set; }

        [DataMember(Name = "dogma_effects")]
        public Dogma_Effects[] Dogma_Effects { get; set; }

        public ItemType()
        {

        }
        public ItemType(long id)
        {
            Id = id;
            this.GetItemType();
        }
    }

    public class Dogma_Attributes
    {

        [DataMember(Name = "attribute_id")]
        public int Id { get; set; }

        [DataMember(Name = "value")]
        public float Value { get; set; }
    }

    public class Dogma_Effects
    {

        [DataMember(Name = "effect_id")]
        public int Id { get; set; }

        [DataMember(Name = "is_default")]
        public bool IsDefault { get; set; }
    }

}
