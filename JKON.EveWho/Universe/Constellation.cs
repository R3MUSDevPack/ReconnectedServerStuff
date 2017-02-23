using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JKON.EveWho.Universe
{

    public class Constellation
    {
        [DataMember(Name = "constellation_id")]
        public long Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "position")]
        public Position Position { get; set; }

        [DataMember(Name = "region_id")]
        public int Region_Id { get; set; }

        [DataMember(Name = "systems")]
        public int[] Systems { get; set; }

        [JsonIgnore]
        public Region Region { get; set; }

        public Constellation()
        {
        }

        public Constellation(long id)
        {
            Id = id;
            this.GetConstellation();
            Region = new Region(Region_Id);
        }
    }

    public class Position
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

}
