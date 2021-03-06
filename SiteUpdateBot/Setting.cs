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
    
    public partial class Setting
    {
        public System.Guid Id { get; set; }
        public string ForumName { get; set; }
        public string ForumUrl { get; set; }
        public Nullable<bool> IsClosed { get; set; }
        public Nullable<bool> EnableRSSFeeds { get; set; }
        public Nullable<bool> DisplayEditedBy { get; set; }
        public Nullable<bool> EnablePostFileAttachments { get; set; }
        public Nullable<bool> EnableMarkAsSolution { get; set; }
        public Nullable<bool> EnableSpamReporting { get; set; }
        public Nullable<bool> EnableMemberReporting { get; set; }
        public Nullable<bool> EnableEmailSubscriptions { get; set; }
        public Nullable<bool> ManuallyAuthoriseNewMembers { get; set; }
        public Nullable<bool> EmailAdminOnNewMemberSignUp { get; set; }
        public Nullable<int> TopicsPerPage { get; set; }
        public Nullable<int> PostsPerPage { get; set; }
        public Nullable<bool> EnablePrivateMessages { get; set; }
        public Nullable<int> MaxPrivateMessagesPerMember { get; set; }
        public Nullable<int> PrivateMessageFloodControl { get; set; }
        public Nullable<bool> EnableSignatures { get; set; }
        public Nullable<bool> EnablePoints { get; set; }
        public Nullable<int> PointsAllowedToVoteAmount { get; set; }
        public Nullable<int> PointsAddedPerPost { get; set; }
        public Nullable<int> PointsAddedPostiveVote { get; set; }
        public Nullable<int> PointsAddedForSolution { get; set; }
        public Nullable<int> PointsDeductedNagativeVote { get; set; }
        public string AdminEmailAddress { get; set; }
        public string NotificationReplyEmail { get; set; }
        public string SMTP { get; set; }
        public string SMTPUsername { get; set; }
        public string SMTPPort { get; set; }
        public Nullable<bool> SMTPEnableSSL { get; set; }
        public string SMTPPassword { get; set; }
        public string Theme { get; set; }
        public Nullable<System.Guid> NewMemberStartingRole { get; set; }
        public System.Guid DefaultLanguage_Id { get; set; }
        public Nullable<int> ActivitiesPerPage { get; set; }
        public Nullable<bool> EnableAkisment { get; set; }
        public string AkismentKey { get; set; }
        public string CurrentDatabaseVersion { get; set; }
        public string SpamQuestion { get; set; }
        public string SpamAnswer { get; set; }
        public Nullable<bool> EnableSocialLogins { get; set; }
        public Nullable<bool> EnablePolls { get; set; }
        public Nullable<bool> NewMemberEmailConfirmation { get; set; }
        public Nullable<bool> SuspendRegistration { get; set; }
        public string PageTitle { get; set; }
        public string MetaDesc { get; set; }
    
        public virtual MembershipRole MembershipRole { get; set; }
    }
}
