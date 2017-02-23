using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JKON.EveWho.Universe
{
    public class SolarSystem
    {
        [DataMember(Name = "system_id")]
        public long Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "position")]
        public Position Position { get; set; }

        [DataMember(Name = "security_status")]
        public float SecurityStatus { get; set; }

        [DataMember(Name = "constellation_id")]
        public long Constellation_Id { get; set; }

        [JsonIgnore]
        public Constellation Constellation { get; set; }

        [DataMember(Name = "planets")]
        public Planet[] Planets { get; set; }

        [DataMember(Name = "stargates")]
        public object[] Stargates { get; set; }
        
        public SolarSystem()
        {

        }
        public SolarSystem(long id)
        {
            Id = id;
            this.GetSolarSystem();
            Constellation = new Constellation(Constellation_Id);
        }
    }
    
    public class Planet
    {
        [DataMember(Name = "planet_id")]
        public int Planet_Id { get; set; }
        [DataMember(Name = "moons")]
        public int[] Moons { get; set; }
    }

}
