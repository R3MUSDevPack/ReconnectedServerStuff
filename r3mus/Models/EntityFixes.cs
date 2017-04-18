using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace r3mus.Models
{
    [MetadataType(typeof(CRONJobMetaData))]
    public partial class CRONJob{ }
    public partial class CRONJobMetaData
    {
        [Key]
        public string JobName { get; set; }
    }

    [MetadataType(typeof(DeclaredToonMetaData))]
    public partial class DeclaredToon { }
    public partial class DeclaredToonMetaData
    {
        [Key]
        [Column(Order = 0)]
        public string User_Id { get; set; }

        [Key]
        [Column(Order = 1)]
        public string ToonName { get; set; }
    }

    [MetadataType(typeof(FleetMetaData))]
    public partial class Fleet { }
    public partial class FleetMetaData
    {
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        //public System.DateTime Time { get; set; }
    }
}