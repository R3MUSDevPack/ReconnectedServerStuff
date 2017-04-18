using r3mus.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace r3mus.ViewModels
{
    public class FleetCreationViewModel
    {
        public Fleet Fleet { get; set; }

        [DataType(DataType.MultilineText)]
        public string Members { get; set; }

        public FleetCreationViewModel()
        {
            Members = string.Empty;
            Fleet = new Fleet();
        }
    }
}