using r3mus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace r3mus.ViewModels
{
    public partial class FleetSummaryViewModel
    {
        public List<Fleet> Fleets { get; set; }
        public List<FleetCountViewModel> Summary { get; set; }
    }
}