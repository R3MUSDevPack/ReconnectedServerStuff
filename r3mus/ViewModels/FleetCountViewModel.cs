using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace r3mus.ViewModels
{
    public class FleetCountViewModel
    {
        public string Commander { get; set; }
        [DisplayName("Fleet Count This Month")]
        public int MonthCount { get; set; }
        [DisplayName("Fleet Count Last Month")]
        public int LastMonthCount { get; set; }
    }
}