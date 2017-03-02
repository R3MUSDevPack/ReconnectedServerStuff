using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace JKON.EveWho.Wars
{
    public class War
    {
        [DataMember(Name = "id")]
        public long Id { get; set; }

        [DataMember(Name = "declared")]
        public DateTime Declared { get; set; }

        [DataMember(Name = "mutual")]
        public bool Mutual { get; set; }

        [DataMember(Name = "open_for_allies")]
        public bool OpenForAllies { get; set; }

        [DataMember(Name = "aggressor")]
        public Aggressor Aggressor { get; set; }

        [DataMember(Name = "defender")]
        public Defender Defender { get; set; }

        [DataMember(Name = "started")]
        public DateTime StartTime { get; set; }

        public War()
        {

        }
        public War(long id)
        {
            Id = id;
            this.GetWar();
        }
        
        public static List<long> GetWars(long? lastWarId)
        {
            return ESI.GetWars().WarIds.Where(warId => (lastWarId == null) || (warId > lastWarId)).ToList();
        }
    }

    public class Aggressor
    {
        [DataMember(Name = "ships_killed")]
        public int ShipsKilled { get; set; }

        [DataMember(Name = "isk_destroyed")]
        public float IskDestroyed { get; set; }

        [OptionalField]
        [DataMember(Name = "alliance_id")]
        public long? Alliance_Id;

        [OptionalField]
        [DataMember(Name = "corporation_id")]
        public long? Corporation_Id;
    }

    public class Defender
    {
        [DataMember(Name = "ships_killed")]
        public int ShipsKilled { get; set; }

        [DataMember(Name = "isk_destroyed")]
        public float IskDestroyed { get; set; }

        [OptionalField]
        [DataMember(Name = "alliance_id")]
        public long? Alliance_Id;

        [OptionalField]
        [DataMember(Name = "corporation_id")]
        public long? Corporation_Id;
    }

    public class Wars
    {
        public long[] WarIds { get; set; }        
    }
}
