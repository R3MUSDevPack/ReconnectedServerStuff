
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace r3mus.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Fleet
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Fleet()
    {

        this.FleetCompositions = new HashSet<FleetComposition>();

    }


    public long Id { get; set; }

    public string Commander { get; set; }

    public System.DateTime Time { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<FleetComposition> FleetCompositions { get; set; }

}

}
