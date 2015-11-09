using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteUpdateBot
{
    [MetadataType(typeof(Announcement_MetaData))]
    public partial class Announcement
    {
    }
    public partial class Announcement_MetaData
    {
        [Key]
        public DateTime Date { get; set; }
    }
}
