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
    
    public partial class ApplicantList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public int ApiKey { get; set; }
        public string VerificationCode { get; set; }
        public string Information { get; set; }
        public string Age { get; set; }
        public string ToonAge { get; set; }
        public string Source { get; set; }
        public Nullable<System.DateTime> Applied { get; set; }
        public Nullable<System.DateTime> LastStatusUpdate { get; set; }
        public string Status { get; set; }
        public System.DateTime DateTimeCreated { get; set; }
        public string Notes { get; set; }
        public string UserName { get; set; }
    }
}