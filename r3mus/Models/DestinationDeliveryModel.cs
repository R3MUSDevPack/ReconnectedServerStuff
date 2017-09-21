using EveAI.Live.Utility;
using System.Collections.Generic;
using System.ComponentModel;

namespace r3mus.Models
{
    public class DestinationDeliveryModel
    {
        public long Id { get; set; }
        [DisplayName("Destination: ")]
        public string Destination { get; set; }
        public List<Contract> DisplayContracts { get; set; }
        [DisplayName("Total Volume: ")]
        public double TotalVolume { get; set; }
    }
}