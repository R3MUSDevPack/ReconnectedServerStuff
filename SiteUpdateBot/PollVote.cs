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
    
    public partial class PollVote
    {
        public System.Guid Id { get; set; }
        public System.Guid MembershipUser_Id { get; set; }
        public System.Guid PollAnswer_Id { get; set; }
    
        public virtual MembershipUser MembershipUser { get; set; }
        public virtual PollAnswer PollAnswer { get; set; }
    }
}
