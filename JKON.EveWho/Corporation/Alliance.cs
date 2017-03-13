using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JKON.EveWho.Corporation
{
    public class Alliance
    {
        private List<long> _corporationIds;
        private List<Corporation> _corps = new List<Corporation>();

        public long Id { get; set; }

        [DataMember(Name = "alliance_name")]
        public string Alliance_Name { get; set; }

        [DataMember(Name = "ticker")]
        public string Ticker { get; set; }

        [DataMember(Name = "date_founded")]
        public DateTime Date_Founded { get; set; }

        [DataMember(Name = "executor_corp")]
        public int Executor_Corp { get; set; }

        [IgnoreDataMember]
        public virtual List<long> CorporationIds
        {
            get
            {
                if (_corporationIds == null) { _corporationIds = this.GetAllianceCorporations().CorpIds; }
                return _corporationIds;
            }
        }

        public Alliance()
        {

        }
        public Alliance(long id)
        {
            Id = id;
            this.GetAlliance();
        }        
    }

}
