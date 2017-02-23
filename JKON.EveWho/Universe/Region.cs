using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JKON.EveWho.Universe
{
    public class Region
    {
        [DataMember(Name = "region_id")]
        public long Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "constellations")]
        public int[] Constellation_Ids { get; set; }

        public Region()
        {
        }

        public Region (long id)
        {
            Id = id;
            this.GetRegion();
        }
    }
}
