
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
    
public partial class FleetComposition
{

    public long Id { get; set; }

    public long FleetId { get; set; }

    public string MemberName { get; set; }



    public virtual Fleet Fleet { get; set; }

}

}