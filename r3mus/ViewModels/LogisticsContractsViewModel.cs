using JKON.EveWho.Models;
using r3mus.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace r3mus.ViewModels
{
    public class LogisticsContractsViewModel
    {
        public Dictionary<long, EveCharacter> CharacterInfos { get; set; }
        public List<DestinationDeliveryModel> DeliveryPoints { get; set; }
        [DisplayName("Total Volume Outstanding: ")]
        public double TotalVolumeOutstanding { get; set; }
    }
}