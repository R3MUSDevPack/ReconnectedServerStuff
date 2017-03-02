using System;
using System.Runtime.Serialization;

namespace JKON.EveWho.Corporation
{
    public class Alliance
    {
        public long Id { get; set; }

        [DataMember(Name = "alliance_name")]
        public string Alliance_Name { get; set; }

        [DataMember(Name = "ticker")]
        public string Ticker { get; set; }

        [DataMember(Name = "date_founded")]
        public DateTime Date_Founded { get; set; }

        [DataMember(Name = "executor_corp")]
        public int Executor_Corp { get; set; }

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
