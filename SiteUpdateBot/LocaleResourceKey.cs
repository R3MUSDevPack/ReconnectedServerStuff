//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SiteUpdateBot
{
    using System;
    using System.Collections.Generic;
    
    public partial class LocaleResourceKey
    {
        public LocaleResourceKey()
        {
            this.LocaleStringResources = new HashSet<LocaleStringResource>();
        }
    
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public System.DateTime DateAdded { get; set; }
    
        public virtual ICollection<LocaleStringResource> LocaleStringResources { get; set; }
    }
}
