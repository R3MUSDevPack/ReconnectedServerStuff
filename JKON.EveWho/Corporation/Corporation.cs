using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JKON.EveWho.Corporation
{     
    public class Corporation
    {
        public long Id { get; set; }

        [DataMember(Name = "alliance_id")]
        public long? Alliance_Id { get; set; }
        [DataMember(Name = "ceo_id")]
        public long CEO_Id { get; set; }
        [DataMember(Name = "corporation_description")]
        public string Corporation_Description { get; set; }
        [DataMember(Name = "corporation_name")]
        public string Corporation_Name { get; set; }
        [DataMember(Name = "creation_date")]
        public DateTime Creation_Date { get; set; }
        [DataMember(Name = "creator_id")]
        public long Creator_Id { get; set; }
        [DataMember(Name = "member_count")]
        public long Member_Count { get; set; }
        [DataMember(Name = "tax_rate")]
        public float Tax_Rate { get; set; }
        [DataMember(Name = "ticker")]
        public string Ticker { get; set; }
        [DataMember(Name = "url")]
        public string Url { get; set; }

        public Corporation()
        {

        }
        public Corporation(long id)
        {
            Id = id;
            this.GetCorporation();
        }
    }

}
